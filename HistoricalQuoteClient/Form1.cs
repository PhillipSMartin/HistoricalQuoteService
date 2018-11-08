using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using HistoricalQuoteClient.HistoricalQuoteReference;

namespace HistoricalQuoteClient
{
    public partial class Form1 : Form
    {
        private HistoricalQuoteServiceClient proxy = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            proxy = new HistoricalQuoteServiceClient("HistoricalQuoteService");
        }

        private void buttonGetQuote_Click(object sender, EventArgs e)
        {
            try
            {
                string ticker = textBox1.Text;
                DateTime date = dateTimePicker1.Value;
                string quoteType = comboBox1.SelectedItem.ToString();

                labelOpen.Text = String.Empty;
                labelClose.Text = String.Empty;
                labelHigh.Text = String.Empty;
                labelLow.Text = String.Empty;
                labelWap.Text = String.Empty;
                labelVolume.Text = String.Empty;
                labelMsg.Text = String.Empty;

                HistoricalQuote quote = proxy.GetQuote(ticker, quoteType, date);

                labelOpen.Text = String.Format("{0}", quote.Open);
                labelClose.Text = String.Format("{0}", quote.Close);
                labelHigh.Text = String.Format("{0}", quote.High);
                labelLow.Text = String.Format("{0}", quote.Low);
                labelWap.Text = String.Format("{0}", quote.Wap);
                labelVolume.Text = String.Format("{0}", quote.Volume);
                labelMsg.Text = String.Format("{0}", quote.ErrorMessage); 
            }
            catch (Exception ex)
            {
                labelMsg.Text = ex.Message;
            }
        }
    }
}
