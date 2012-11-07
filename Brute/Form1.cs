using System;
using System.Collections.Generic;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
//using Org.Mentalis.Utilities;
using System.Threading;
using System.Security.Cryptography;
using System.Data.OleDb;
using System.Data.Odbc;
using System.IO;

namespace Brute
{
    public partial class Form1 : Form
    {
        #region Variables
        //private static CpuUsage cpu;
        System.Threading.Timer t = null;
        // int cpuLoad = 0;

        DateTime startTime;
        TimeSpan duration;

        ArrayList characterArray = new ArrayList();
        int min = 1;
        int max = 14;

        double total = 0;
        int current = 0;
        int avarage = 0;
        int previous = 0;
        int maxSpeed = 0;

        double numVals = 0;
        bool incDone = false;
        string realMax = "";
        string currentVal = "";
        string firstVal = "";
        string startsWith = "";

        bool domd5 = false;
        bool dosha1 = false;
        bool md5Found = false;
        bool sha1Found = false;
        string md5inputhash = "";
        string sha1inputhash = "";
        string md5hash = "";
        string sha1hash = "";

        string testPW = "";

        int i = 0;
        int j = 0;

        bool cancelWorker = false;
        // bool printPassword = false;

        string[] passwordArray;

        string[] lower = { "a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
        string[] upper = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
        string[] digits = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };

        private delegate void tsControl(Control target, string value);
        tsControl updateControl;
        int globalcount = 0;

        #endregion

        #region Enums
        public enum HashType : int { MD5, SHA1, SHA256, SHA384, SHA512 }
        #endregion

        public Form1()
        {
            InitializeComponent();
            updateControl = new tsControl(UpdateControlCall);

            setupCPULoad();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = 0;
            comboBox2.SelectedIndex = 9;
            comboBox3.SelectedIndex = 0;
            txtStartsWith.Text = getProgress();
        }

        #region Methods

        public void UpdateControlCall(Control targetControl, string targetValue)
        {
            if (targetControl.InvokeRequired) //Invoke Method
            {
                try
                {
                    targetControl.Invoke(updateControl, new object[] { targetControl, targetValue });
                }
                catch (Exception) { }
            }
            else
            {
                if (targetControl.GetType() == typeof(Label))
                {
                    targetControl.Text = targetValue;
                }
                if (targetControl.GetType() == typeof(ProgressBar))
                {
                    ProgressBar tmpControl = new ProgressBar();
                    tmpControl = (ProgressBar)targetControl;
                    Int32 tmpInt = Convert.ToInt32(targetValue);
                    if (tmpInt >= tmpControl.Minimum && tmpInt <= tmpControl.Maximum)
                    {
                        tmpControl.Value = Convert.ToInt32(targetValue);
                    }
                }
            }
        }

        private void setupCharArray()
        {
            if (ckhLower.Checked)
            {
                characterArray.AddRange(lower);
            }
            if (chkUpper.Checked)
            {
                characterArray.AddRange(upper);
            }
            if (chkDigits.Checked)
            {
                characterArray.AddRange(digits);
            }
        }

        private void brute()
        {
            //Setup the password array
            passwordArray = new string[max];
            //reset the count to Zero

            globalcount = 0;

            for (i = 0; i < max; i++)
            { passwordArray[i] = "-1"; }

            //Get the amount of characters
            numVals = characterArray.Count;

            //Calculate the total possible characters
            total = Math.Pow(numVals, max);

            //Get the real maximum
            for (i = 0; i < max + 1; i++)
            { realMax = realMax + characterArray[Convert.ToInt32(numVals - 1)]; }

            //Start setting the pw string
            for (i = 0; i < min; i++)
            {
                passwordArray[i] = characterArray[0].ToString();
            }

            //Add the starts with value.
            startsWith = txtStartsWith.Text;

            if (startsWith != string.Empty)
            {
                for (int y = 0; y < startsWith.Length; y++)
                {
                    passwordArray[y] = startsWith.Substring(y, 1);
                }
            }


            i = 0;

            //Set the First Value
            while (passwordArray[i] != "-1" && cancelWorker == false)
            {
                firstVal += passwordArray[i];
                i++;

                current++;
                //updateControl(label4, current + " of " + total);
            }
            //ok this is the first update pass
            testPW = firstVal;
            updatePassword();

            while (true && cancelWorker == false)
            {
                for (i = 0; i < (max + 1); i++)
                {

                    if (passwordArray[i] == "-1")
                    {
                        break;
                    }
                }

                i--;
                incDone = false;
                while (!incDone)
                {
                    for (j = 0; j < numVals; j++)
                    {
                        try
                        {
                            if (passwordArray[i] == characterArray[j].ToString())
                            {
                                break;
                            }
                        }
                        catch (Exception) { }
                    }

                    if (j == (numVals - 1))
                    {
                        passwordArray[i] = characterArray[0].ToString();
                        updatePassword(); ;

                        current++;
                        //updateControl(label4, current + " of " + total);

                        i--;

                        if (i < 0)
                        {
                            for (i = 0; i < (max + 1); i++)
                            {
                                if (passwordArray[i] == "-1")
                                {
                                    break;
                                }
                            }
                            passwordArray[i] = characterArray[0].ToString();
                            passwordArray[i + 1] = "-1";
                            updatePassword();

                            current++;
                            //updateControl(label4, current + " of " + total);

                            incDone = true;
                        }
                    }
                    else
                    {
                        //same place?
                        try
                        {
                            passwordArray[i] = characterArray[j + 1].ToString();
                        }
                        catch (Exception) { }
                        string tempPW = passwordString();
                        Thread up = new Thread(() => updatePassword(tempPW));
                        //updatePassword();

                        up.Start();

                        current++;
                        //updateControl(label4, current + " of " + total);

                        incDone = true;
                    }
                }
            }

            i = 0;
            currentVal = "";

            while (passwordArray[i] != "-1")
            {
                currentVal = currentVal + passwordArray[i];
                i++;

                testPW = currentVal;
                //here is teh call to display the password
                // updateControl(label1, passwordString());

                if (currentVal == realMax)
                {
                    break;
                }
            }
        }

        private void bruteUI()
        {
            //Setup the password array
            passwordArray = new string[max];

            for (i = 0; i < max; i++)
            { passwordArray[i] = "-1"; }

            //Get the amount of characters
            numVals = characterArray.Count;

            //Calculate the total possible characters
            total = Math.Pow(numVals, max);

            //Get the real maximum
            for (i = 0; i < max + 1; i++)
            { realMax = realMax + characterArray[Convert.ToInt32(numVals - 1)]; }

            //Start setting the pw string
            for (i = 0; i < min; i++)
            {
                passwordArray[i] = characterArray[0].ToString();
            }

            //Add the starts with value.
            startsWith = txtStartsWith.Text;

            if (startsWith != string.Empty)
            {
                for (int y = 0; y < startsWith.Length; y++)
                {
                    passwordArray[y] = startsWith.Substring(y, 1);
                }
            }

            i = 0;

            //Set the First Value
            while (passwordArray[i] != "-1" && cancelWorker == false)
            {
                firstVal += passwordArray[i];
                i++;

                current++;
                label4.Text = current + " of " + total;
                Application.DoEvents();
            }

            testPW = firstVal;
            updatePassword();

            while (true && cancelWorker == false)
            {
                for (i = 0; i < (max + 1); i++)
                {

                    if (passwordArray[i] == "-1")
                    {
                        break;
                    }
                }

                i--;
                incDone = false;
                while (!incDone)
                {
                    for (j = 0; j < numVals; j++)
                    {
                        if (passwordArray[i] == characterArray[j].ToString())
                        {
                            break;
                        }
                    }

                    if (j == (numVals - 1))
                    {
                        passwordArray[i] = characterArray[0].ToString();
                        updatePassword(); ;

                        current++;
                        label4.Text = current + " of " + total;

                        i--;

                        if (i < 0)
                        {
                            for (i = 0; i < (max + 1); i++)
                            {
                                if (passwordArray[i] == "-1")
                                {
                                    break;
                                }
                            }
                            passwordArray[i] = characterArray[0].ToString();
                            passwordArray[i + 1] = "-1";
                            updatePassword();

                            current++;
                            label4.Text = current + " of " + total;
                            Application.DoEvents();

                            incDone = true;
                        }
                    }
                    else
                    {
                        passwordArray[i] = characterArray[j + 1].ToString();

                        updatePassword();

                        current++;
                        label4.Text = current + " of " + total;
                        Application.DoEvents();

                        incDone = true;
                    }
                }
            }

            i = 0;
            currentVal = "";

            while (passwordArray[i] != "-1")
            {
                currentVal = currentVal + passwordArray[i];
                i++;

                testPW = currentVal;

                //label1.Text = passwordString();
                Application.DoEvents();

                if (currentVal == realMax)
                {
                    break;
                }
            }
        }

        private void updatePassword()
        {
            string tempPW = passwordString();
            globalcount++;

            //    updateControl(label1, tempPW);

            //update the password field every 500
            if ((globalcount % 500) == 0)
            {
                updateControl(label1, tempPW);
            }

            updateControl(label1, tempPW);
            if (IsDBPass(tempPW))
            {
                MessageBox.Show("GOT IT!" + tempPW);
                updateControl(txtInputPW, tempPW);
                CancelSearch();


            }


        }
        private void updatePassword(string myPASS)
        {

            //updateControl(label1, myPASS);

            //if ((globalcount % 100) == 0)
            //{
            updateControl(label1, myPASS);
            //}


            if (IsDBPass(myPASS))
            {
                CancelSearch();
                MessageBox.Show("GOT IT!    " + myPASS);
                SavePass(myPASS);
                updateControl(txtInputPW, myPASS);
                updateControl(label1, myPASS);



            }

        }

        private void SavePass(string myPASS)
        {
            // create a writer and open the file
            TextWriter tw = new StreamWriter(@"C:\tmp\gotit.txt");
            // write a line of text to the file
            tw.WriteLine(myPASS);

            // close the stream
            tw.Close();
        }
        private void SaveProgress(string myPass)
        {
            // create a writer and open the file
            TextWriter tw = new StreamWriter(@"C:\tmp\brute.txt");
            // write a line of text to the file
            tw.WriteLine(myPass);

            // close the stream
            tw.Close();
        }
        private string getProgress()
        {
            string tmp = "";

            try
            {
                TextReader tr = new StreamReader(@"C:\tmp\brute.txt");

                // read all of text in file
                tmp = tr.ReadToEnd();
                tr.Close();
                tr.Dispose();


                // close the stream
            }
            catch (Exception) { }


            return tmp.Trim();
        }

        private string passwordString()
        {
            string tmppw = "";

            for (int x = 0; x < max; x++)
            {
                tmppw += passwordArray[x];
            }

            return tmppw.Replace("-1", "");
        }
        OdbcConnection myConnection = new OdbcConnection();

        public bool IsDBPass(string myPass)
        {
            // string conn = @"Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq=C:\brute\pass_test.accdb;Uid=Admin;Pwd=[PASS];";
            //string conn = @"Driver={Microsoft Access Driver (*.mdb, *.accdb)};Dbq=C:\tmp\Keys.accdb;Uid=Admin;Pwd=[PASS];";
            //string conn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\tmp\Keys.accdb;Jet OLEDB:Database Password=[PASS];";

            string conn = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=C:\tmp\Keys.accdb;Jet OLEDB:Database Password=" + myPass + ";";
            //conn = conn.Replace("[PASS]", myPass);

            bool foundit = false;

            OleDbConnection myConnection2 = new OleDbConnection();
            try
            {

                if (myConnection2.State != ConnectionState.Connecting)
                {
                    myConnection2.ConnectionString = conn;
                }
                myConnection2.Open();
                //conn.Open();
                foundit = true;
            }
            catch (Exception)
            {
                //    // MessageBox.Show(ex.ToString());
                return false;
            }
            finally
            {
                myConnection2.Close();
            }
            return foundit;

        }



        private void computeHASHes(string tmpPW)
        {
            if (domd5 && md5Found == false)
            {
                md5hash = Hash.GetHash(tmpPW, Hash.HashType.MD5);
                if (md5inputhash == md5hash)
                {
                    md5Found = true;
                    updateControl(lblMD5pw, tmpPW);
                    updateControl(lblMD5, md5hash);
                }
            }

            if (dosha1 && sha1Found == false)
            {
                sha1hash = Hash.GetHash(tmpPW, Hash.HashType.SHA1);
                if (sha1inputhash == sha1hash)
                {
                    sha1Found = true;
                    updateControl(lblSHA1pw, tmpPW);
                    updateControl(lbSHA1, sha1hash);
                }
            }

            if (sha1Found && md5Found)
            {
                cancelWorker = true;
            }
        }

        #endregion

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            brute();
            // recurse(1, 0, "");
        }

        #region Control Events

        private void button1_Click(object sender, EventArgs e)
        {
            grpSettings.Enabled = false;
            grpHash.Enabled = false;
            setupCharArray();
            cancelWorker = false;

            ////sha1inputhash = textBox1.Text;
            ////md5inputhash = textBox2.Text;

            //dosha1 = false;
            //domd5 = false;

            //if (sha1inputhash != string.Empty)
            //{
            //    dosha1 = true;
            //}

            //if (md5inputhash != string.Empty)
            //{
            //    domd5 = true;
            //}

            startTime = DateTime.Now;

            if (checkBox1.Checked)
            {
                backgroundWorker1.RunWorkerAsync();
            }
            else
            {
                bruteUI();
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CancelSearch();
        }

        private void CancelSearch()
        {

            Action action = () => grpSettings.Enabled = true;
            this.Invoke(action);

            action = () => grpHash.Enabled = true;
            this.Invoke(action);

            action = () => characterArray.Clear();
            this.Invoke(action);

            cancelWorker = true;
            backgroundWorker1.CancelAsync();
        }


        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            min = comboBox1.SelectedIndex + 1;
        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
        {
            max = comboBox2.SelectedIndex + 1;
            txtStartsWith.MaxLength = max;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (comboBox3.SelectedIndex == 0) //SHA1
            {
                textBox1.Text = Hash.GetHash(txtInputPW.Text, Hash.HashType.SHA1);
            }
            else //MD5
            {
                textBox2.Text = Hash.GetHash(txtInputPW.Text, Hash.HashType.MD5);
            }
        }

        private void txtStartsWith_TextChanged(object sender, EventArgs e)
        {
            comboBox1.SelectedIndex = txtStartsWith.Text.Length - 1;
        }

        #endregion


        private void setupCPULoad()
        {
            try
            {

                // cpu = CpuUsage.Create();
                t = new System.Threading.Timer(new TimerCallback(TimerFunction), null, 0, 1000);
            }
            catch (Exception)
            {
                //  MessageBox.Show(e.ToString());
                // Console.WriteLine(e);
            }
        }

        // protected PerformanceCounter cpuCounter = new PerformanceCounter();

        public int getCPUCOunter()
        {

            var cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            cpuCounter.NextValue();
            System.Threading.Thread.Sleep(1000);
            return (int)cpuCounter.NextValue();

        }




        private void TimerFunction(object o)
        {


            int cpuPercent = getCPUCOunter();

            // cpuLoad = Convert.ToInt32(cpu.Query().ToString());
            //  updateControl(progressBar1, cpuLoad.ToString());

            updateControl(progressBar1, cpuPercent.ToString());

            // updateControl(lblCPULoad, cpuLoad.ToString() + "%");

            if (current > 0 && cancelWorker == false)
            {
                avarage = current - previous;
                previous = current;
                if (avarage > maxSpeed)
                {
                    maxSpeed = avarage;
                }
                updateControl(lblAvarage, avarage + " pw/Sec");
                updateControl(lblMax, maxSpeed + " pw/Sec");

                DateTime currentTime = DateTime.Now;

                duration = currentTime - startTime;
                updateControl(lblTime, (duration.Hours.ToString() + ":" + duration.Minutes.ToString() + ":" + duration.Seconds.ToString()));
            }
        }


        private void button4_Click(object sender, EventArgs e)
        {
            //string myTxt = txtStartsWith.Text;
            //MessageBox.Show(IsDBPass(myTxt).ToString());

            //Thread t = new Thread(() => recurse(3, 0, ""));
            //recurse(1, 0, "");
            //            t.Start();

            int cpu = getCPUCOunter();
            MessageBox.Show(cpu.ToString());


        }

        char[] chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z' };

        private void recurse(int width, int position, string baseString)
        {
            for (int i = 0; i < 35; i++)
            {

                if (position < width - 1)
                {

                    recurse(width, position + 1, baseString + chars[i]);

                }

                //checkPassword(baseString + chars[i]);
                string tempPW = (baseString + chars[i]);
                //updateControl(label1, tempPW);
                bool gotIT = IsDBPass(tempPW);

                if (gotIT)
                {
                    MessageBox.Show("Found the password: " + tempPW);
                    button2_Click(gotIT, EventArgs.Empty);
                    updateControl(txtInputPW, tempPW);
                    //  txtInputPW.Text = tempPW;
                }




            }



        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                string tmp = passwordString();
                SaveProgress(tmp);
            }
            catch (Exception) { }
        }
    }
}