using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using ImageMagick;

namespace HeicToJpeg
{
    public class Program : Form
    {
        private ProgressBar progressBar;
        private Label dragDropLabel;

        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Program());
        }

        public Program()
        {
            // �t�H�[���̃^�C�g����ݒ�
            this.Text = "HeicToJpeg";

            // ���x�����쐬
            dragDropLabel = new Label();
            dragDropLabel.Text = "�����ɉ摜���h���b�O �A���h �h���b�v";
            dragDropLabel.AutoSize = true;

            // ���x�����t�H�[���̒����ɔz�u
            dragDropLabel.Location = new System.Drawing.Point(
                ((this.ClientSize.Width - dragDropLabel.Width) / 2) - 30,  // ���{��Y���̕␳��-30
                (this.ClientSize.Height - dragDropLabel.Height) / 2
            );

            // ���x�����t�H�[���ɒǉ�
            this.Controls.Add(dragDropLabel);

            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form_DragEnter);
            this.DragDrop += new DragEventHandler(Form_DragDrop);

            // �i���o�[��ǉ�
            progressBar = new ProgressBar();
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Location = new System.Drawing.Point(10, this.ClientSize.Height - 30);
            progressBar.Width = this.ClientSize.Width - 20;
            this.Controls.Add(progressBar);
            
            // �t�H�[���̃T�C�Y�ύX���ɕ\���v�f�̈ʒu���Ē���
            this.Resize += Program_Resize;
       }

        private void Program_Resize(object sender, EventArgs e)
        {
            dragDropLabel.Location = new System.Drawing.Point(
                (this.ClientSize.Width - dragDropLabel.Width) / 2,
                (this.ClientSize.Height - dragDropLabel.Height) / 2
            );
            
            progressBar.Width = this.ClientSize.Width - 20;
            progressBar.Location = new System.Drawing.Point(10, this.ClientSize.Height - 30);
        }

        private void Form_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
            }
        }

        private async void Form_DragDrop(object sender, DragEventArgs e)
        {
            dragDropLabel.Text = "�摜��ϊ���";
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            var heicFiles = files.Where(file => Path.GetExtension(file).ToLower() == ".heic").ToArray();

            progressBar.Value = 0;
            progressBar.Maximum = heicFiles.Length;

            var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 4 };

            await Task.Run(() =>
            {
                Parallel.ForEach(heicFiles, parallelOptions, (file, state, index) =>
                {
                    ConvertHeicToJpg(file);
                    
                    this.Invoke(new Action(() =>
                    {
                        progressBar.Value += 1;
                    }));

                });
            });

            dragDropLabel.Text = "Converted ALL Images!!";
        }

        private void ConvertHeicToJpg(string heicPath)
        {
            string jpgPath = Path.ChangeExtension(heicPath, ".jpg");

            using (MagickImage image = new MagickImage(heicPath))
            {
                // Exif����ێ�����JPEG�ɕϊ�
                image.Write(jpgPath, MagickFormat.Jpeg);
            }
        }
    }
}
