using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Collections;

namespace OS_5
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            listView1.ListViewItemSorter = new Order();
            //Oper.sstf(100, new int[] { 55, 58, 39, 18, 90, 150, 160, 184, 38 });
            Oper.scan(100, new int[] { 55, 58, 39, 18, 90, 150, 160, 184, 38 }, 2);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            new FileOpen().readFile(listView1);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ListViewItem item = listView1.SelectedItems[0];
                listView1.Items.Remove(item);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (FileOpen.errorDetect(new string[] { textBox2.Text }))
            {
                int nextindex = 0;
                
                for (int i = 0; ;i++)
                {
                    if (i >= listView1.Items.Count || listView1.Items[i].SubItems[0].Text != i.ToString())
                    {
                        nextindex = i;
                        break;
                    }
                }
                ListViewItem list = listView1.Items.Add(nextindex.ToString());
                list.SubItems.Add(textBox2.Text);
            }
                
            else
                MessageBox.Show("输入错误，请检查！");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            listView2.Items.Clear();
            listView3.Items.Clear();
            int init_index = int.Parse(textBox1.Text);
            int[] nums = new int[listView1.Items.Count];

            for (int i = 0; i < nums.Length; i++)
            {
                nums[i] = int.Parse(listView1.Items[i].SubItems[1].Text);
            }

            int flag = checkBox1.Checked ? 0 : 1;
            Disk_res[] sstf_res = Oper.sstf(init_index, nums);
            Disk_res[] scan_res = Oper.scan(init_index, nums, flag);

            int sum = 0;
            int sum2 = 0;

            for (int i = 0; i < sstf_res.Length; i++)
            {
                ListViewItem list = listView2.Items.Add(sstf_res[i].index.ToString());
                list.SubItems.Add(sstf_res[i].value.ToString());
                list.SubItems.Add(sstf_res[i].dv.ToString());
                sum += sstf_res[i].dv;
            }

            for (int i = 0; i < scan_res.Length; i++)
            {
                ListViewItem list = listView3.Items.Add(scan_res[i].index.ToString());
                list.SubItems.Add(scan_res[i].value.ToString());
                list.SubItems.Add(scan_res[i].dv.ToString());
                sum2 += scan_res[i].dv;
            }

            label9.Text = sum.ToString();
            label11.Text = sum2.ToString();

            label10.Text = ((float)sum / (float)(sstf_res.Length)).ToString();
            label12.Text = ((float)sum2 / (float)(scan_res.Length)).ToString();

        }
    }

    public class Oper
    {
        public static Disk_res[] sstf(int init_index, int[] nums)
        {
            Disk_res[] res = new Disk_res[nums.Length];

            int minDv = Int32.MaxValue;
            int minIndex = -1;
            int nowIndex = init_index;
            int[] isVisited = new int[65535];

            // 对数组进行初始化
            for (int i = 0; i < isVisited.Length; i++) isVisited[i] = 0;
            for (int i = 0; i < nums.Length; i++) isVisited[nums[i]]++;

            // 遍历所有磁盘，找到离当前磁头最近且未被访问的磁盘号并访问
            for (int i = 0; i < nums.Length; i++)
            {
                for (int j = 0; j < nums.Length; j++)
                {
                    if (isVisited[nums[j]] == 0) continue;
                    if (Math.Abs(nowIndex - nums[j]) < minDv)
                    {
                        minDv = Math.Abs(nowIndex - nums[j]);
                        minIndex = j;
                    }
                }
                // 新建结果，记录移动的距离、移动到的位置等信息
                Disk_res disk = new Disk_res();
                disk.dv = minDv;
                disk.index = minIndex;
                disk.value = nums[minIndex];
                res[i] = disk;

                // 修改磁头以及访问记录
                nowIndex = disk.value;
                minDv = Int32.MaxValue;
                isVisited[nowIndex]--;
            }
            return res;
        }

        public static Disk_res[] scan(int init_index, int[] nums, int mode = 0)
        {
            // mode=0 先访问大的，else访问小的先
            Disk_res[] res = new Disk_res[nums.Length];

            int minDv = Int32.MaxValue;
            int minIndex = -1;
            int nowIndex = init_index;
            bool[] isVisited = new bool[65535];
            bool isEqual = false;
            int equalCipan = 0;
            int equalIndex = 0;

            Disk_index[] disk = new Disk_index[nums.Length];
            for (int i = 0; i < nums.Length; i++)
            {
                Disk_index temp = new Disk_index();
                temp.cipan = nums[i];
                temp.index = i;
                disk[i] = temp;
            }

            Array.Sort<Disk_index>(disk);

            int split = 0;
            for (int i = 0; i < nums.Length; i++)
            {
                if (init_index == disk[i].cipan)
                {
                    split = i + 1;
                    isEqual = true;
                    equalCipan = nums[i];
                    equalIndex = i;
                    break;
                }
                if (i > 0 && init_index < disk[i].cipan && init_index > disk[i - 1].cipan)
                {
                    split = i;
                    break;
                }
            }

            Disk_index[] disk_small;
            if (isEqual)
            {
                disk_small = new Disk_index[split - 1];
            }
            else
            {
                disk_small = new Disk_index[split];
            }
                
                
            Disk_index[] disk_big = new Disk_index[nums.Length - split];

            if (!isEqual)
            {
                for (int i = 0; i < nums.Length; i++)
                {
                    if (i < split)
                    {
                        disk_small[i] = disk[i];
                    }
                    else
                    {
                        disk_big[i - split] = disk[i];
                    }
                }
            }
            else
            {
                for (int i = 0; i < nums.Length; i++)
                {
                    if (i < split - 1)
                    {
                        disk_small[i] = disk[i];
                    }
                    else if (i >= split)
                    {
                        disk_big[i - split] = disk[i];
                    }
                }
            }

            int i1 = 0;
            if (isEqual)
            {
                Disk_res disk_ = new Disk_res();
                disk_.dv = 0;
                disk_.index = equalIndex;
                disk_.value = equalCipan;

                nowIndex = disk[i1].cipan;

                res[i1] = disk_;
                i1++;
            }

            if (mode == 0)
            {
                for (int i = 0; i < disk_big.Length; i1++, i++)
                {
                    Disk_res disk_ = new Disk_res();
                    disk_.dv = Math.Abs(nowIndex - disk_big[i].cipan);
                    disk_.index = disk_big[i].index;
                    disk_.value = disk_big[i].cipan;

                    nowIndex = disk_big[i].cipan;

                    res[i1] = disk_;
                }

                for (int i = disk_small.Length - 1; i >= 0; i1++, i--)
                {
                    Disk_res disk_ = new Disk_res();
                    disk_.dv = Math.Abs(nowIndex - disk_small[i].cipan);
                    disk_.index = disk_small[i].index;
                    disk_.value = disk_small[i].cipan;

                    nowIndex = disk_small[i].cipan;

                    res[i1] = disk_;
                }
            }

            else
            {
                for (int i = disk_small.Length - 1; i >= 0; i1++, i--)
                {
                    Disk_res disk_ = new Disk_res();
                    disk_.dv = Math.Abs(nowIndex - disk_small[i].cipan);
                    disk_.index = disk_small[i].index;
                    disk_.value = disk_small[i].cipan;

                    nowIndex = disk_small[i].cipan;

                    res[i1] = disk_;
                }

                for (int i = 0; i < disk_big.Length; i1++, i++)
                {
                    Disk_res disk_ = new Disk_res();
                    disk_.dv = Math.Abs(nowIndex - disk_big[i].cipan);
                    disk_.index = disk_big[i].index;
                    disk_.value = disk_big[i].cipan;

                    nowIndex = disk_big[i].cipan;

                    res[i1] = disk_;
                }
            }
            return res;
        }
    }

    public class Disk_res
    {
        public int dv;      // 移动距离
        public int index;   // 编号
        public int value;   // 移动后的值
        public Disk_res()
        {
            dv = 0;
            index = 0;
        }
    }

    public class Disk_index : IComparable<Disk_index>
    {
        public int index;
        public int cipan;

        public Disk_index()
        {
            index = 0;
            cipan = 0;
        }

        public int CompareTo(Disk_index other)
        {
            if (this.cipan < other.cipan) return -1;
            else if (this.cipan == other.cipan) return 0;
            else return 1;
        }
    }

    class FileOpen
    {
        private string selectPath()
        {
            string path = string.Empty;
            OpenFileDialog openFileDialog = new OpenFileDialog()
            {
                Filter = "Files (*.txt)|*.txt"//如果需要筛选txt文件（"Files (*.txt)|*.txt"）
            };

            //var result = openFileDialog.ShowDialog();
            if (openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                path = openFileDialog.FileName;
            }

            return path;
        }

        private int countColcu(int[] a)
        {
            int count = -1;
            for (int i = 0; i < 100; i++)
            {
                if (a[i] == 0)
                {
                    a[i] = 1;
                    count = i + 1;
                    break;
                }
            }
            return count;
        }

        public static bool digitjdg(string x)
        {
            const string pattern = "^[0-9]*$";
            Regex rx = new Regex(pattern);
            bool IsDigit = rx.IsMatch(x);
            return IsDigit;//是数字返回true,不是返回false
        }

        public static bool errorDetect(string[] s)
        {
            foreach (string str in s)
            {
                if (!digitjdg(str)) return false;
            }
            return true;
        }
        public void readFile(ListView listView)
        {
            string path = selectPath();
            if (path == String.Empty)
            {
                MessageBox.Show("读取失败！");
                return;
            }
            StreamReader sr = new StreamReader(path, Encoding.Default);

            String line;
            while ((line = sr.ReadLine()) != null)
            {
                string[] s = line.Split('\t');

                if (!errorDetect(s))
                {
                    MessageBox.Show("读取错误！请检查后输入。");
                }

                int nextindex = 0;

                for (int i = 0; ; i++)
                {
                    if (i >= listView.Items.Count || listView.Items[i].SubItems[0].Text != i.ToString())
                    {
                        nextindex = i;
                        break;
                    }
                }
                ListViewItem list = listView.Items.Add(nextindex.ToString());
                list.SubItems.Add(line);




            }
            MessageBox.Show("读取成功！");
        }
    }

    class Order : IComparer
    {
        public int Compare(Object x, Object y)
        {
            int xx = int.Parse(((ListViewItem)x).SubItems[0].Text);
            int yy = int.Parse(((ListViewItem)y).SubItems[0].Text);
            if (xx < yy) return -1;
            if (xx == yy) return 0;
            else return 1;
        }

    }
}
