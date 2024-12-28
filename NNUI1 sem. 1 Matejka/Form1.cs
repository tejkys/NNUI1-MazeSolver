using System.Xml.Linq;

namespace NNUI1_sem._1_Matejka
{
    public partial class Form1 : Form
    {
        public Maze Maze { get; set; } = new();
        public Form1()
        {
            InitializeComponent();
            Maze.UpdateUi = (maze) =>
            {
                label8.Invoke(new MethodInvoker(delegate { label8.Text = maze.Agent.Cost.ToString(); }));
                label10.Invoke(new MethodInvoker(delegate { label10.Text = maze.FoundSolutin.ToString(); }));
                pictureBox2.Image = maze.Visualization;
            };
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog dlg = new OpenFileDialog())
            {
                dlg.Title = "Open Image";
                dlg.Filter = "bmp file (*.bmp)|*.bmp";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    ImageHasChanged(dlg.FileName);
                }
            }
        }
        private async void ImageHasChanged(string filename = null)
        {
            Bitmap bitmap;
            if (string.IsNullOrEmpty(filename))
            {
                bitmap = new Bitmap(Properties.Resources.Bludiste1);

            }
            else
            {
                bitmap = new Bitmap(filename);
            }
            pictureBox1.Image = bitmap;
            Maze.BitmapImage = bitmap;
            await Maze.BuildVisualBitmap();
            pictureBox2.Image = Maze.Visualization;
            numericUpDown2.Maximum = Maze.BitmapImage.Width - 1;
            numericUpDown4.Maximum = Maze.BitmapImage.Width - 1;
            numericUpDown1.Maximum = Maze.BitmapImage.Height - 1;
            numericUpDown3.Maximum = Maze.BitmapImage.Height - 1;
            label2.Text = $"{Maze.BitmapImage.Width} x {Maze.BitmapImage.Height}";

            if (string.IsNullOrEmpty(filename))
            {
                numericUpDown2.Value = 11;
                numericUpDown1.Value = 31;
                numericUpDown4.Value = 21;
                numericUpDown3.Value = 1;
            }
            else {
                numericUpDown2.Value = 0;
                numericUpDown1.Value = 0;
                numericUpDown4.Value = 0;
                numericUpDown3.Value = 0;
            }
        }
        private async void Form1_Load(object sender, EventArgs e)
        {
            ImageHasChanged();
        }

        private async void numericUpDown_ValueChanged(object sender, EventArgs e)
        {
            await Maze.SetStartPoint((int)numericUpDown2.Value, (int)numericUpDown1.Value);
            await Maze.SetEndPoint((int)numericUpDown4.Value, (int)numericUpDown3.Value);
            pictureBox2.Image = Maze.Visualization;
        }

		private void button2_Click(object sender, EventArgs e)
		{
            button2.Enabled = false;

            this.Invoke(new Action(
                async delegate ()
                {
                    await Maze.FindSolutionAsync();
                    button2.Enabled = true;
                }));
        }

    }
}