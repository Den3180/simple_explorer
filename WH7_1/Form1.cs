using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace WH7_1
{
    public partial class Form1 : Form
    {
        ImageList galery, galeryLarge;
        long size = 0; 

        public Form1()
        {
            InitializeComponent();
            FillDriveNodes();
            SettingImage();
            treeView1.Select();
            treeView1.SelectedNode = treeView1.Nodes[0];
            TextInfo(treeView1.Nodes[0]);
        }
        //Настройка отображения иконок для разных режимов.
        private void SettingImage()
        {
            galery = new ImageList();
            galeryLarge = new ImageList();
            treeView1.ImageList = galery;
            galery.ImageSize = new Size(20, 20);
            galery.ColorDepth = ColorDepth.Depth24Bit;
            galery.TransparentColor = Color.FromArgb(128, 128, 128);
            galeryLarge.TransparentColor = Color.FromArgb(128, 128, 128);
            galery.Images.Add(new Bitmap("Disk.bmp"));
            galery.Images.Add(new Bitmap("Folder2.bmp"));
            galery.Images.Add(new Bitmap("Folder1.bmp"));
            galery.Images.Add(new Bitmap("File.bmp"));
            galeryLarge.Images.Add(new Bitmap("Disk.bmp"));
            galeryLarge.Images.Add(new Bitmap("Folder2.bmp"));
            galeryLarge.Images.Add(new Bitmap("Folder1.bmp"));
            galeryLarge.Images.Add(new Bitmap("File.bmp"));
        }

        //Поиск и добавление логичеких дисков.
        private void FillDriveNodes()
        {
            try
            {
                foreach (DriveInfo drive in DriveInfo.GetDrives())
                {
                    TreeNode driveNode = new TreeNode { Text = drive.Name };
                    treeView1.Nodes.Add(driveNode);
                    FillTreeNode(driveNode, drive.Name);
                }
            }
            catch (Exception ex) { }
        }

        //Поиск вложенных каталогов,файлов и добавление их в коллекцию.
        private void FillTreeNode(TreeNode driveNode, string path)
        {
            try
            {
                string[] dirs = Directory.GetDirectories(path);

                foreach (string dir in dirs)
                {
                    TreeNode dirNode = new TreeNode();
                    dirNode.Text = dir;
                    driveNode.Nodes.Add(dirNode);
                }
                dirs = Directory.GetFiles(path); //Поиск файлов в папке.
                if (dirs!=null) 
                {
                    foreach (string dir in dirs) //Добавление файлов.
                    {
                        TreeNode dirNode = new TreeNode();
                        dirNode.Text = dir;
                        driveNode.Nodes.Add(dirNode);
                    }
                }
            }
            catch (Exception ex) {}
        }

        //Развертывание узлов дерева.
        private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            e.Node.Nodes.Clear(); //Очистка TreeView.
            if (!check_Drives(e.Node)) //Если это не верхушка дерева (логические диски).
            {
                e.Node.ImageIndex = 2;              //Используем рисунок открытой папки в случае когда узел не выделен.
                e.Node.SelectedImageIndex = 2;      //Используем рисунок открытой папки в случае когда узел выделен.
            }
            string[] dirs;
            TreeNode dirNode;
            FileInfo[] files;
            try
            {
                if (Directory.Exists(e.Node.FullPath)) //Если директория существует.
                {
                    dirs = Directory.GetDirectories(e.Node.FullPath); //Получаем имена подкаталогов.
                    if (dirs.Length != 0)
                    {
                        for (int i = 0; i < dirs.Length; i++)
                        {
                            dirNode = new TreeNode(new DirectoryInfo(dirs[i]).Name);
                            dirNode.ImageIndex = 1;
                            dirNode.SelectedImageIndex = 1;
                            FillTreeNode(dirNode, dirs[i]); //Находим вложенные подкаталоги.
                            e.Node.Nodes.Add(dirNode);      //Добавляем в коллекцию.
                        }
                    }
                    files = new DirectoryInfo(e.Node.FullPath).GetFiles(); //Отображение файлов при развертывании.
                    foreach (FileInfo file in files)
                    {
                        dirNode = new TreeNode(file.Name);
                        dirNode.ImageIndex = 3;
                        dirNode.SelectedImageIndex = 3;
                        e.Node.Nodes.Add(dirNode);
                       
                    }
                }
            }
            catch (Exception ex) { }
        }

        //Свертывание узлов дерева.
        private void treeView1_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
        {
            if (!check_Drives(e.Node)) //Если это не верхушка дерева (логические диски).
            {
            e.Node.SelectedImageIndex = 1; //Используем рисунок закрытой папки в случае когда узел выделен.
            e.Node.ImageIndex = 1;         //Используем рисунок закрытой папки в случае когда узел не выделен.
            }
        }

        //Выбор режима отображения в ListView.
        private bool check_Drives(TreeNode node)
        {
            DriveInfo[] dr = DriveInfo.GetDrives();//Список логических дисков.
            foreach (DriveInfo dir in dr) //Обход и проверка на совпадение с текущим узлом.
            {
                if (dir.Name == node.Text)
                {
                    return true;
                }
            }
            return false;
        }

        //Происходит при выборе элемента дерева.
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            try
            {
                if (Directory.Exists(e.Node.FullPath)||File.Exists(e.Node.FullPath))//Если такая директория/файл существует.
                {
                    TextInfo(e.Node); //Поиск и вывод информации об элементе.
                }
            }
            catch (Exception ex) { }
        }

        //Поиск и вывод информации об элементе.
        private void TextInfo(TreeNode node)
        {
            size = 0; //Размер элемента.
            DirectoryInfo directoryInfo=null;
            FileInfo[] files=null;
            FileInfo file = null;
            listView2.Clear(); //Очистка listview.
            directoryInfo = new DirectoryInfo(node.FullPath);
            file = new FileInfo(node.FullPath);
            if (!file.Exists)                      
                files = directoryInfo.GetFiles();  //Если выбрана папка или диск забираем список файлов в нем.


            if (check_Drives(node))  //Информация о логическом диске.
            {
                DriveInfo drive = new DriveInfo(node.FullPath);
                    listView2.Items.Add("Имя: " + node.Text);
                    listView2.Items.Add("Тип: логический диск ");
                    listView2.Items.Add("Количество элементов: " + (node.Nodes.Count + files.Length).ToString());
                    listView2.Items.Add("Дата создания: "+directoryInfo.CreationTime.ToShortDateString());
                    listView2.Items.Add("Размер: " + drive.TotalSize.ToString()+" байт");
            }
            
            else
            {
                listView2.Items.Add("Имя: " + node.Text);

                if (directoryInfo.Exists) //Информация о каталоге.
                {
                    files = directoryInfo.GetFiles();
                    listView2.Items.Add("Тип: папка с файлами ");
                    listView2.Items.Add("Количество элементов: " + (node.Nodes.Count + files.Length).ToString());
                    listView2.Items.Add("Дата создания: " + directoryInfo.CreationTime.ToShortDateString());
                    listView2.Items.Add("Атрибуты: " + directoryInfo.Attributes.ToString());
                    listView2.Items.Add("Размер: " + SizeDirectory(node.FullPath).ToString()+" байт");
                }
                else if(file.Exists)  //Информация о файле.
                {
                    listView2.Items.Add("Тип: файл ");
                    listView2.Items.Add("Дата создания: "+file.CreationTime.ToShortDateString());
                    listView2.Items.Add("Атрибуты: "+file.Attributes.ToString());
                    listView2.Items.Add("Размер: "+file.Length+" байт");
                }
            }
         }
       
        private long SizeDirectory(string path) //Вычисление размера каталога.Рекурсивный метод.
        {
            FileInfo file = null;
          
                string[] listPath = Directory.GetFileSystemEntries(path);
                foreach (string i in listPath)
                {
                    if (File.Exists(i))
                    {
                        file = new FileInfo(i);
                        size += file.Length;
                    }
                    if (Directory.Exists(i))
                    {
                        SizeDirectory(i);
                    }
                }
            return size;
        }
    }
}
