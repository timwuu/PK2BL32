using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Pk2 = PICkit2V2.PICkitFunctions;
using KONST = PICkit2V2.Constants;

namespace PICkit2V2
{
    public partial class DialogUnitSelect : Form
    {
        public DialogUnitSelect()
        {
            string unitID;
            ushort i;

            InitializeComponent();
            this.Size = new Size(this.Size.Width, (int)(FormPICkit2.ScalefactH * this.Size.Height));
            
            // Find up to 8 PICkit 2 Units.
            for (i = 0; i < 8; i++)
            {
                KONST.PICkit2USB detRes = Pk2.DetectPICkit2Device(i, false, false);
            
                if (detRes != KONST.PICkit2USB.notFound)
                { // found something
                    /*if (detRes == KONST.PICkit2USB.bootloader)
                    {
                        listBoxUnits.Items.Add("  " + i.ToString() + "                <Bootloader>");
                    }
                    else if (detRes == Constants.PICkit2USB.firmwareInvalid)
                    { // min FW for UnitID is 2.10
                        if ((Pk2.FirmwareVersion[0] == '2') && (ushort.Parse(Pk2.FirmwareVersion.Substring(2,2)) >= 10))
                        {
                            string unitID = Pk2.UnitIDRead();
                            if (unitID == "")
                                unitID = "-";
                            listBoxUnits.Items.Add("  " + i.ToString() + "                " + unitID);                        
                        }
                        else
                        {
                            listBoxUnits.Items.Add("  " + i.ToString() + "                <FW v" + Pk2.FirmwareVersion + ">");
                        }
                    
                    }
                    else
                    {
                        string unitID = Pk2.UnitIDRead();
                        if (unitID == "")
                            unitID = "-";
                        listBoxUnits.Items.Add("  " + i.ToString() + "                " + unitID); 
                    }*/

                    unitID = Pk2.GetSerialUnitID();
                    if (unitID == "PIC18F2550") unitID = "<bootloader>";
                    listBoxUnits.Items.Add("  " + i.ToString() + "                " + unitID); 
                
                }
                else
                {
                    break;
                }
            }

            KONST.PICkit2USB detRes2 = Pk2.DetectPICkit2Device(0, true, false);

            if (detRes2 != KONST.PICkit2USB.notFound)
            {
                unitID = "0X00BT";
                listBoxUnits.Items.Add("  " + i.ToString() + "                " + unitID); 
            }

        }

        private void listBoxUnits_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            FormPICkit2.pk2number = (ushort)listBoxUnits.SelectedIndex;
            this.Close();
        }

        private void listBoxUnits_SelectedIndexChanged(object sender, EventArgs e)
        {
            buttonSelectUnit.Enabled = true;
        }

        private void buttonSelectUnit_Click(object sender, EventArgs e)
        {
            string s = listBoxUnits.SelectedItem.ToString();
            if (s.Substring( s.Length-2, 2) == "BT")
            {
                FormPICkit2.pk2number = 0;
                FormPICkit2.pk2BT = true;
            }
            else
            {
                FormPICkit2.pk2number = (ushort)listBoxUnits.SelectedIndex;
                FormPICkit2.pk2BT = false;
            }
            this.Close();
        }
    }
}