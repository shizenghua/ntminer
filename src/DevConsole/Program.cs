﻿using NTMiner.NoDevFee;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace NTMiner {
    internal unsafe class Program {
        private static bool s_running = true;
        private static string s_poolIp;
        private static bool s_ranOnce = false;

        private static void Main(string[] args) {
            Console.CancelKeyPress += delegate { s_running = false; };

            if (args.Length >= 1) {
                s_poolIp = args[0];
            }
            else {
                Console.WriteLine("ERROR: No poolIp argument was found.");
                Console.WriteLine("按任意键退出");
                Console.ReadKey();
                return;
            }
            if (args.Length >= 2) {
                Console.Title = args[1] + "开始时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff");
            }
            else {
                Console.Title = "开始时间：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff");
            }

            WinDivertExtract.Extract();

            string filter = $"ip && (ip.DstAddr = {s_poolIp} || ip.SrcAddr = {s_poolIp}) && tcp && tcp.PayloadLength > 100";
            Console.WriteLine(filter);
            var divertHandle = WinDivertNativeMethods.WinDivertOpen(filter, WINDIVERT_LAYER.WINDIVERT_LAYER_NETWORK, 0, 0);

            try {
                if (divertHandle != IntPtr.Zero) {
                    Parallel.ForEach(Enumerable.Range(0, Environment.ProcessorCount), x => RunDiversion(divertHandle, ref s_ranOnce, ref s_poolIp, ref s_running));
                }
            }
            catch (Exception e) {
                Console.WriteLine(e.Message, e.StackTrace);
            }
            finally {
                WinDivertNativeMethods.WinDivertClose(divertHandle);
            }
        }

        private static void RunDiversion(IntPtr handle, ref bool ranOnce, ref string poolIp, ref bool running) {
            byte[] packet = new byte[65535];
            try {
                while (running) {
                    uint readLength = 0;
                    WINDIVERT_IPHDR* ipv4Header = null;
                    WINDIVERT_TCPHDR* tcpHdr = null;
                    WINDIVERT_ADDRESS addr = new WINDIVERT_ADDRESS();

                    if (!WinDivertNativeMethods.WinDivertRecv(handle, packet, (uint)packet.Length, ref addr, ref readLength)) continue;

                    if (!ranOnce && readLength > 1) {
                        ranOnce = true;
                        Console.WriteLine("Diversion running..");
                    }

                    fixed (byte* inBuf = packet) {
                        byte* payload = null;
                        WinDivertNativeMethods.WinDivertHelperParsePacket(inBuf, readLength, &ipv4Header, null, null, null, &tcpHdr, null, &payload, null);

                        if (ipv4Header != null && tcpHdr != null && payload != null) {
                            string text = Marshal.PtrToStringAnsi((IntPtr)payload);
                            string dstIp = ipv4Header->DstAddr.ToString();
                            var dstPort = tcpHdr->DstPort;
                            string arrow = $"->{dstIp}:{dstPort}";
                            if (dstIp == poolIp) {
                                arrow = $"{dstIp}:{dstPort}<-";
                                Console.WriteLine($"<-<-<-<-<-<-<-<-<-<-<-<-<-{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}<-<-<-<-<-<-<-<-<-<-<-<-<-<-<-");
                            }
                            else {
                                Console.WriteLine($"->->->->->->->->->->->->->{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss fff")}->->->->->->->->->->->->->->->");
                            }
                            Console.WriteLine(arrow + text);
                            Console.WriteLine();
                            Console.WriteLine();
                        }
                    }

                    WinDivertNativeMethods.WinDivertHelperCalcChecksums(packet, readLength, 0);
                    WinDivertNativeMethods.WinDivertSendEx(handle, packet, readLength, 0, ref addr, IntPtr.Zero, IntPtr.Zero);
                }

            }
            catch (Exception e) {
                Console.WriteLine(e.ToString());
                Console.WriteLine("按任意键退出");
                Console.ReadKey();
                return;
            }
        }
    }
}
