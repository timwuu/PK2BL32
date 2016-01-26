using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.IO;

namespace PICkit2V2
{
    public class scriptCmdList
    {
        public class cmd
        {
            public string name;
            public byte id;
            public int narg;
            public bool plusN;

            public cmd(string _name, int _narg, bool _plusN)
            {
                name = _name;
                narg = _narg;
                plusN = _plusN;
            }
        }

        //new cmd("SETVDD",3,false), //0xA0
        static cmd[] cmds = new cmd[] { 
            new cmd("JT2_PE_PROG_RESP",0,false),//0xB3
            new cmd("JT2_WAIT_PE_RESP",0,false),//0xB4
            new cmd("JT2_GET_PE_RESP",0,false),//0xB5
            new cmd("JT2_XFERINST_BUF",0,false),//0xB6
            new cmd("JT2_XFRFASTDAT_BUF",0,false),//0xB7
            new cmd("JT2_XFRFASTDAT_LIT",4,false),//0xB8
            new cmd("JT2_XFERDATA32_LIT",4,false),//0xB9
            new cmd("JT2_XFERDATA8_LIT",1,false),//0xBA
            new cmd("JT2_SENDCMD",1,false),//0xBB
            new cmd("JT2_SETMODE",2,false),//0xBC
            new cmd("UNIO_TX_RX",3,false),//0xBD
            new cmd("UNIO_TX",2,false),//0xBE
            new cmd("MEASURE_PULSE",0,false),//0xBF
            new cmd("ICDSLAVE_TX_BUF_BL",0,false),//0xC0
            new cmd("ICDSLAVE_TX_LIT_BL",1,false),//0xC1
            new cmd("ICDSLAVE_RX_BL",0,false),//0xC2
            new cmd("SPI_RDWR_BYTE_BUF",0,false),//0xC3
            new cmd("SPI_RDWR_BYTE_LIT",1,false),//0xC4
            new cmd("SPI_RD_BYTE_BUF",0,false),//0xC5
            new cmd("SPI_WR_BYTE_BUF",0,false),//0xC6
            new cmd("SPI_WR_BYTE_LIT",1,false),//0xC7
            new cmd("I2C_RD_BYTE_NACK",0,false),//0xC8
            new cmd("I2C_RD_BYTE_ACK",0,false),//0xC9
            new cmd("I2C_WR_BYTE_BUF",0,false),//0xCA
            new cmd("I2C_WR_BYTE_LIT",1,false),//0xCB
            new cmd("I2C_STOP",0,false),//0xCC
            new cmd("I2C_START",0,false),//0xCD
            new cmd("AUX_STATE_BUFFER",0,false),//0xCE
            new cmd("SET_AUX",1,false),//0xCF
            new cmd("WRITE_BITS_BUF_HLD",1,false),//0xD0
            new cmd("WRITE_BITS_LIT_HLD",2,false),//0xD1
            new cmd("CONST_WRITE_DL",1,false),//0xD2
            new cmd("WRITE_BUFBYTE_W",1,false),//0xD3
            new cmd("WRITE_BUFWORD_W",1,false),//0xD4
            new cmd("RD2_BITS_BUFFER",1,false),//0xD5
            new cmd("RD2_BYTE_BUFFER",0,false),//0xD6
            new cmd("VISI24",0,false),//0xD7
            new cmd("NOP24",0,false),//0xD8
            new cmd("COREINST24",3,false),//0xD9
            new cmd("COREINST18",2,false),//0xDA
            new cmd("POP_DOWNLOAD",0,false),//0xDB
            new cmd("ICSP_STATES_BUFFER",0,false),//0xDC
            new cmd("LOOPBUFFER",1,false),//0xDD
            new cmd("ICDSLAVE_TX_BUF",0,false),//0xDE
            new cmd("ICDSLAVE_TX_LIT",1,false),//0xDF
            new cmd("ICDSLAVE_RX",0,false),//0xE0
            new cmd("POKE_SFR",2,false),//0xE1
            new cmd("PEEK_SFR",1,false),//0xE2
            new cmd("EXIT_SCRIPT",0,false),//0xE3
            new cmd("GOTO_INDEX",1,false),//0xE4
            new cmd("IF_GT_GOTO",2,false),//0xE5
            new cmd("IF_EQ_GOTO",2,false),//0xE6
            new cmd("DELAY_SHORT",1,false),//0xE7
            new cmd("DELAY_LONG",1,false),//0xE8
            new cmd("LOOP",2,false),//0xE9
            new cmd("SET_ICSP_SPEED",1,false),//0xEA
            new cmd("READ_BITS",1,false),//0xEB
            new cmd("READ_BITS_BUFFER",1,false),//0xEC
            new cmd("WRITE_BITS_BUFFER",1,false),//0xED
            new cmd("WRITE_BITS_LITERAL",2,false),//0xEE
            new cmd("READ_BYTE",0,false),//0xEF
            new cmd("READ_BYTE_BUFFER",0,false),//0xF0
            new cmd("WRITE_BYTE_BUFFER",0,false),//0xF1
            new cmd("WRITE_BYTE_LITERAL",1,false),//0xF2
            new cmd("SET_ICSP_PINS",1,false),//0xF3
            new cmd("BUSY_LED_OFF",0,false),//0xF4
            new cmd("BUSY_LED_ON",0,false),//0xF5
            new cmd("MCLR_GND_OFF",0,false),//0xF6
            new cmd("MCLR_GND_ON",0,false),//0xF7
            new cmd("VPP_PWM_OFF",0,false),//0xF8
            new cmd("VPP_PWM_ON",0,false),//0xF9
            new cmd("VPP_OFF",0,false),//0xFA
            new cmd("VPP_ON",0,false),//0xFB
            new cmd("VDD_GND_OFF",0,false),//0xFC
            new cmd("VDD_GND_ON",0,false),//0xFD
            new cmd("VDD_OFF",0,false),//0xFE
            new cmd("VDD_ON",0,false)//0xFF
                                      };

        public static int getCmdString(TextWriter tw, byte[] cmdlist, int ind)
        {
            byte cmdID = cmdlist[ind];
            byte id1;
            cmd cmd1;
            int i;

            if (cmdID > 0xB2)
            {
                id1 = (byte)(cmdID - 0xB3);
                cmd1 = cmds[id1];
                tw.WriteLine();
                tw.Write(" $");
                tw.Write(cmd1.name);

                for (i = 0; i < cmd1.narg; i++)
                {
                    tw.Write(' ');
                    tw.Write(cmdlist[ind + i + 1].ToString("X2"));
                }

                return cmd1.narg;

            }
            else
            {
                tw.WriteLine("ERR");
            }
            return 0;
        }

    }
}
