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
            // フォームのタイトルを設定
            this.Text = "HeicToJpeg";

            // ラベルを作成
            dragDropLabel = new Label();
            dragDropLabel.Text = "ここに画像をドラッグ アンド ドロップ";
            dragDropLabel.AutoSize = true;

            // ラベルをフォームの中央に配置
            dragDropLabel.Location = new System.Drawing.Point(
                ((this.ClientSize.Width - dragDropLabel.Width) / 2) - 30,  // 日本語ズレの補正で-30
                (this.ClientSize.Height - dragDropLabel.Height) / 2
            );

            // ラベルをフォームに追加
            this.Controls.Add(dragDropLabel);

            this.AllowDrop = true;
            this.DragEnter += new DragEventHandler(Form_DragEnter);
            this.DragDrop += new DragEventHandler(Form_DragDrop);

            // 進捗バーを追加
            progressBar = new ProgressBar();
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
            progressBar.Location = new System.Drawing.Point(10, this.ClientSize.Height - 30);
            progressBar.Width = this.ClientSize.Width - 20;
            this.Controls.Add(progressBar);
            
            // フォームのサイズ変更時に表示要素の位置を再調整
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
            dragDropLabel.Text = "画像を変換中";
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
                // Exif情報を保持してJPEGに変換
                image.Write(jpgPath, MagickFormat.Jpeg);
            }
        }
    }
}
