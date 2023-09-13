using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MatrixEncryption
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        bool decryptMode = false;
        double[,] keyMatrix;
        int columnAmount = 0;
        static double determinant;
        bool decimalsAllowed = false;
        bool useTextMatrix = false;
        bool convertToUnicode = false;


        private void button2_Click(object sender, EventArgs e)
        {
            if (!decryptMode)
            {
                decryptMode = true;
                button1.Text = "Decrypt Text";
                label1.Text = "Text to Decrypt";
                label2.Text = "Decrypted Text";
                label3.Text = "Decryption Mode";
            }
            else
            {
                decryptMode = false;
                button1.Text = "Encrypt Text";
                label1.Text = "Text to Encrypt";
                label2.Text = "Encrypted Text";
                label3.Text = "Encryption Mode";
            }

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!decryptMode) //Encrypt Mode
            {
                string textInput = textBox1.Text;
                List<char> stringToChar = new List<char>();
                List<int> charToUTF16 = new List<int>();
                stringToChar.AddRange(textInput);
                foreach (char x in stringToChar)
                {
                    int UTF16Value = Convert.ToInt32(x);
                    charToUTF16.Add(UTF16Value);
                }
                columnAmount = (int)Math.Ceiling((double)charToUTF16.Count / 3);
                int[,] messageMatrix = new int[3, columnAmount];
                double[,] encryptedMatrix = new double[3, columnAmount];
                int i = 0;
                int j = 0;
                foreach (int x in charToUTF16)
                {
                    messageMatrix[j, i] = x;
                    if (j == 2)
                    {
                        j = 0;
                        i++;
                    }
                    else
                    {
                        j++;
                    }
                }
                for (int k = 0; k < 3; k++)
                {
                    for (int m = 0; m < columnAmount; m++)
                    {
                        encryptedMatrix[k, m] =
                            keyMatrix[k, 0] * messageMatrix[0, m] +
                            keyMatrix[k, 1] * messageMatrix[1, m] +
                            keyMatrix[k, 2] * messageMatrix[2, m];
                        if (encryptedMatrix[k, m] != Math.Round(encryptedMatrix[k, m]) && convertToUnicode)
                        {
                            convertToUnicode = false;
                            MessageBox.Show("Key contains decimal numbers. Encryption will be displayed as decimals instead.");
                            checkBox2.Checked = false;
                        }
                        if ((encryptedMatrix[k, m] > 65536 || encryptedMatrix[k,m] < 0) && convertToUnicode)
                        {
                            convertToUnicode = false;
                            MessageBox.Show("Encryption out of UTF16 bounds. Encryption will be displayed as numbers instead.");
                            checkBox2.Checked = false;
                        }
                    }
                }
                string encryptedString = "";
                if (!convertToUnicode)
                {
                    for (int m = 0; m < columnAmount; m++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            encryptedString += encryptedMatrix[k, m] + " ";
                        }
                    }
                }
                else
                {
                    List<char> unicodeList = new List<char>();
                    for (int m = 0; m < columnAmount; m++)
                    {
                        for (int k = 0; k < 3; k++)
                        {
                            double y = encryptedMatrix[k, m];
                            char z = Convert.ToChar(Convert.ToInt32(y));
                            unicodeList.Add(z);
                        }
                    }
                    foreach (char x in unicodeList)
                    {
                        encryptedString += x;
                    }
                }                           
                textBox2.Text = encryptedString;
            }
            else //Decrypt Mode
            {
                string textInput = textBox1.Text;
                List<double> inputDouble = new List<double>();
                try
                {
                    if (convertToUnicode)
                    {
                        foreach (char x in textInput)
                        {
                            inputDouble.Add(Convert.ToDouble(Convert.ToInt32(x)));
                        }
                    }
                    else
                    {
                        string[] codeInput = textInput.Split(' ');
                        foreach (string x in codeInput)
                        {
                            string y = x;
                            if (!y.Contains('.'))
                            {
                                y += ".0";
                            }
                            inputDouble.Add(Convert.ToDouble(y));
                        }
                    }
                }              
                catch (FormatException)
                {
                    MessageBox.Show("Invalid character(s) inputted.");
                    return;
                }
                columnAmount = (int)Math.Ceiling((double)inputDouble.Count / 3);
                double[,] encryptedMatrix = new double[3, columnAmount];
                int i = 0;
                int j = 0;
                foreach (double x in inputDouble)
                {
                    encryptedMatrix[j, i] = x;
                    if (j == 2)
                    {
                        j = 0;
                        i++;
                    }
                    else
                    {
                        j++;
                    }
                    if ((x > 65536 || x < 0) && convertToUnicode)
                    {
                        convertToUnicode = false;
                        MessageBox.Show("Encryption out of UTF16 bounds. Encryption will be displayed as numbers instead.");
                        checkBox2.Checked = false;
                    }
                }

                double[,] inverseKey = InvertMatrix(keyMatrix);
                double[,] decryptedMatrix = new double[3, columnAmount];
                for (int k = 0; k < 3; k++)
                {
                    for (int m = 0; m < columnAmount; m++)
                    {
                        decryptedMatrix[k, m] =
                            inverseKey[k, 0] * encryptedMatrix[0, m] +
                            inverseKey[k, 1] * encryptedMatrix[1, m] +
                            inverseKey[k, 2] * encryptedMatrix[2, m];
                    }
                }
                List<int> UTF16List = new List<int>();
                int p = 0;
                int q = 0;
                bool inUTF16Bounds = true;
                for (int r = 0; r < (3 * columnAmount); r++)
                {
                    int y = (int)Math.Round(decryptedMatrix[q, p]);
                    UTF16List.Add(y);
                    if (q == 2)
                    {
                        q = 0;
                        p++;
                    }
                    else
                    {
                        q++;
                    }
                }
                if (!inUTF16Bounds)
                {
                    MessageBox.Show("One or more characters were not in ascii bounds. Incorrect key was inputted or encrpyted characters are not supported.");
                }
                string decryptedOutput = "";
                foreach (int x in UTF16List)
                {
                    if (inUTF16Bounds == true)
                    {
                        char y = Convert.ToChar(x);
                        decryptedOutput += y;
                    }
                    else
                    {
                        decryptedOutput += Convert.ToString(x);
                    }
                }
                textBox2.Text = decryptedOutput;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                double keyMatrix00 = 0;
                double keyMatrix01 = 0;
                double keyMatrix02 = 0;
                double keyMatrix10 = 0;
                double keyMatrix11 = 0;
                double keyMatrix12 = 0;
                double keyMatrix20 = 0;
                double keyMatrix21 = 0;
                double keyMatrix22 = 0;
                if (!useTextMatrix)
                {
                    keyMatrix00 = Convert.ToDouble(textBox3.Text);
                    keyMatrix01 = Convert.ToDouble(textBox4.Text);
                    keyMatrix02 = Convert.ToDouble(textBox5.Text);
                    keyMatrix10 = Convert.ToDouble(textBox6.Text);
                    keyMatrix11 = Convert.ToDouble(textBox7.Text);
                    keyMatrix12 = Convert.ToDouble(textBox8.Text);
                    keyMatrix20 = Convert.ToDouble(textBox9.Text);
                    keyMatrix21 = Convert.ToDouble(textBox10.Text);
                    keyMatrix22 = Convert.ToDouble(textBox11.Text);
                }
                else
                {
                    string textMatrixInput = textBox14.Text;
                    string[] textMatrixArray = textMatrixInput.Split(' ');
                    if (!textMatrixArray.Contains("{") || !textMatrixArray.Contains("}") || textMatrixArray.Length != 11)
                    {
                        MessageBox.Show("Invalid matrix text inputted.");
                        textBox14.Text = "Text Key Input";
                        textBox14.ForeColor = Color.Gray;
                        return;
                    }
                    List<string> textMatrixList = new List<string>();
                    foreach (string x in textMatrixArray)
                    {
                        if (x != "{" && x != "}")
                        {
                            textMatrixList.Add(x);
                        }
                    }
                    keyMatrix00 = Convert.ToDouble(textMatrixList[0]);
                    keyMatrix01 = Convert.ToDouble(textMatrixList[1]);
                    keyMatrix02 = Convert.ToDouble(textMatrixList[2]);
                    keyMatrix10 = Convert.ToDouble(textMatrixList[3]);
                    keyMatrix11 = Convert.ToDouble(textMatrixList[4]);
                    keyMatrix12 = Convert.ToDouble(textMatrixList[5]);
                    keyMatrix20 = Convert.ToDouble(textMatrixList[6]);
                    keyMatrix21 = Convert.ToDouble(textMatrixList[7]);
                    keyMatrix22 = Convert.ToDouble(textMatrixList[8]);
                    textBox3.Text = Convert.ToString(keyMatrix00);
                    textBox4.Text = Convert.ToString(keyMatrix01);
                    textBox5.Text = Convert.ToString(keyMatrix02);
                    textBox6.Text = Convert.ToString(keyMatrix10);
                    textBox7.Text = Convert.ToString(keyMatrix11);
                    textBox8.Text = Convert.ToString(keyMatrix12);
                    textBox9.Text = Convert.ToString(keyMatrix20);
                    textBox10.Text = Convert.ToString(keyMatrix21);
                    textBox11.Text = Convert.ToString(keyMatrix22);                    
                }
                string keyString = "{ " + keyMatrix00 + " " + keyMatrix01 + " " + keyMatrix02 +
                               " " + keyMatrix10 + " " + keyMatrix11 + " " + keyMatrix12 +
                               " " + keyMatrix20 + " " + keyMatrix21 + " " + keyMatrix22 + " }";
                textBox14.ForeColor = Color.Black;
                textBox14.Text = keyString;

                keyMatrix = new double[3, 3] {
                    { keyMatrix00, keyMatrix01, keyMatrix02 },
                    { keyMatrix10, keyMatrix11, keyMatrix12 },
                    { keyMatrix20, keyMatrix21, keyMatrix22 } };

                InvertMatrix(keyMatrix);
                if (determinant == 0)
                {
                    MessageBox.Show("Invalid key inputted. Matrix does not have an inverse.");
                }
                else
                {
                    button1.Enabled = true;
                }
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid character(s) inputted.");
                textBox14.Text = "Text Key Input";
                textBox14.ForeColor = Color.Gray;
                useTextMatrix = false;
            }
        }


        public static double[,] InvertMatrix(double[,] matrix)
        {
            double x, y, z, x1, y1, z1, x2, y2, z2;
            determinant = matrix[0, 0] * (matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1])
                    - matrix[0, 1] * (matrix[1, 0] * matrix[2, 2] - matrix[1, 2] * matrix[2, 0])
                    + matrix[0, 2] * (matrix[1, 0] * matrix[2, 1] - matrix[2, 0] * matrix[1, 1]);
            x = (matrix[1, 1] * matrix[2, 2] - matrix[1, 2] * matrix[2, 1]);
            y = -1 * (matrix[1, 0] * matrix[2, 2] - matrix[1, 2] * matrix[2, 0]);
            z = (matrix[1, 0] * matrix[2, 1] - matrix[2, 0] * matrix[1, 1]);

            x1 = -1 * (matrix[0, 1] * matrix[2, 2] - matrix[0, 2] * matrix[2, 1]);
            y1 = (matrix[0, 0] * matrix[2, 2] - matrix[2, 0] * matrix[0, 2]);
            z1 = -1 * (matrix[0, 0] * matrix[2, 1] - matrix[0, 1] * matrix[2, 0]);

            x2 = (matrix[0, 1] * matrix[1, 2] - matrix[1, 1] * matrix[0, 2]);
            y2 = -1 * (matrix[0, 0] * matrix[1, 2] - matrix[1, 0] * matrix[0, 2]);
            z2 = (matrix[0, 0] * matrix[1, 1] - matrix[1, 0] * matrix[0, 1]);

            double t = y;

            y = x1;
            x1 = t;
            t = z;
            z = x2;
            x2 = t;
            t = z1;
            z1 = y2;
            y2 = t;

            double[,] nm = new double[3, 3] {
                { x / determinant, y / determinant, z / determinant },
                { x1 / determinant, y1 / determinant, z1 / determinant },
                { x2 / determinant, y2 / determinant, z2 / determinant } };
            return nm;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            Random rand = new Random();
            double minRandom;
            double maxRandom;
            if (checkBox1.Checked)
            {
                decimalsAllowed = true;
            }
            else
            {
                decimalsAllowed = false;
            }
            try
            {
                minRandom = Convert.ToDouble(textBox12.Text);
                maxRandom = Convert.ToDouble(textBox13.Text);
            }
            catch (FormatException)
            {
                MessageBox.Show("Invalid character(s) inputted.");
                return;
            }
            if (minRandom >= maxRandom)
            {
                MessageBox.Show("Minimum value greater than or equal to maximum value.");
                return;
            }
            List<double> randDouble = new List<double>();
            if (!decimalsAllowed)
            {
                for (int i = 0; i < 9; i++)
                {
                    double randGen = rand.Next(Convert.ToInt32(minRandom), Convert.ToInt32(maxRandom + 1));
                    randDouble.Add(randGen);
                }
            }
            else
            {
                for (int i = 0; i < 9; i++)
                {
                    double randBase = rand.Next(Convert.ToInt32(minRandom), Convert.ToInt32(maxRandom + 1));
                    double addedDecimal = rand.NextDouble();
                    double randfullDecimal = randBase + addedDecimal;
                    if (numericUpDown1.Value != 14)
                    {
                        randfullDecimal = Math.Round(randfullDecimal, (int)numericUpDown1.Value);
                    }
                    randDouble.Add(randfullDecimal);
                }
            }
            textBox3.Text = Convert.ToString(randDouble[0]);
            textBox4.Text = Convert.ToString(randDouble[1]);
            textBox5.Text = Convert.ToString(randDouble[2]);
            textBox6.Text = Convert.ToString(randDouble[3]);
            textBox7.Text = Convert.ToString(randDouble[4]);
            textBox8.Text = Convert.ToString(randDouble[5]);
            textBox9.Text = Convert.ToString(randDouble[6]);
            textBox10.Text = Convert.ToString(randDouble[7]);
            textBox11.Text = Convert.ToString(randDouble[8]);
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            useTextMatrix = false;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            useTextMatrix = false;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            useTextMatrix = false;
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            useTextMatrix = false;
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            useTextMatrix = false;
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            useTextMatrix = false;
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            useTextMatrix = false;
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            useTextMatrix = false;
        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {
            useTextMatrix = false;
        }

        private void textBox14_TextChanged(object sender, EventArgs e)
        {
            useTextMatrix = true;
        }

        private void textBox12_Enter(object sender, EventArgs e)
        {
            textBox12.ForeColor = Color.Black;
            if (textBox12.Text == "Min")
            {
                textBox12.Text = "";
            }
            
        }

        private void textBox13_Enter(object sender, EventArgs e)
        {
            textBox13.ForeColor = Color.Black;
            if (textBox13.Text == "Max")
            {
                textBox13.Text = "";
            }
        }

        private void textBox12_Leave(object sender, EventArgs e)
        {
            if (textBox12.Text == "")
            {
                textBox12.Text = "Min";
                textBox12.ForeColor = Color.Gray;
            }
            else
            {
                textBox12.ForeColor = Color.Black;
            }
        }

        private void textBox13_Leave(object sender, EventArgs e)
        {
            if (textBox13.Text == "")
            {
                textBox13.Text = "Max";
                textBox13.ForeColor = Color.Gray;
            }
            else
            {
                textBox12.ForeColor = Color.Black;
            }

        }

        private void textBox14_Enter(object sender, EventArgs e)
        {
            textBox14.ForeColor = Color.Black;
            if (textBox14.Text == "Text Key Input")
            {
                textBox14.Text = "";
            }
        }

        private void textBox14_Leave(object sender, EventArgs e)
        {
            if (textBox14.Text == "")
            {
                textBox14.Text = "Text Key Input";
                textBox14.ForeColor = Color.Gray;
            }
            else
            {
                textBox12.ForeColor = Color.Black;
            }
            if (textBox14.Text == "" || textBox14.Text == "Text Key Input")
            {
                useTextMatrix = false;
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox14.Text);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            textBox14.Text = "";
            textBox14.ForeColor = Color.Black;
            textBox14.Paste();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            textBox1.Text = "";
            textBox1.Paste();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(textBox2.Text);
        }

        private void checkBox2_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox2.Checked)
            {
                convertToUnicode = true; 
            }
            else
            {
                convertToUnicode = false;
            }
        }
    }
}
