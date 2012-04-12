﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading.Tasks;
using AmericasCup.Data;
using System.Diagnostics;

namespace AmericasCup.Feed
{
    public class Client
    {
        Socket _Socket;
        NetworkStream _Stream;

        public event Action<byte[], byte[], byte[]> OnMessage;

        public void Connect()
        {
            _Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _Socket.Connect("157.125.69.155", 4940);

            _Stream = new NetworkStream(_Socket, System.IO.FileAccess.Read);

            Action<byte[]> fillbuffer = b =>
            {
                int total = 0, read = 0;
                while ((read = _Stream.Read(b, total, b.Length - total)) > 0)
                {
                    total += read;
                }
            };

            Action receive = null;
            receive = new Action(() =>
             {
                 var header = new byte[15];
                 fillbuffer(header);
                 
                 var c = BitConverter.ToUInt16(header, 13);
                 var body = new byte[c];
                 fillbuffer(body);

                 var crc = new byte[4];
                 fillbuffer(crc);

                 //uint cm = BitConverter.ToUInt32(crc, 0);
                 //uint c1 = Crc32.Compute(header.Concat(body).ToArray());

                 //if (c1 != cm)
                 //{
                 //    Debug.WriteLine(string.Format("CRC check failed: {1} in message vs. {0} calculated", c1, cm));
                 //}

                 string sheader = string.Join(" ", header.Select(b => b.ToString("X2")));
                 string sbody = string.Join(" ", body.Select(b => b.ToString("X2")));
                 string scrc = string.Join(" ", crc.Select(b => b.ToString("X2")));
                 Debug.Write(string.Format("Header: {0}\nBody: {1}\nCRC: {2}\n", sheader, sbody, scrc));

                 Task.Factory.StartNew(() =>
                 {
                     if (OnMessage != null) OnMessage(header, body, crc);
                     Task.Factory.StartNew(receive);
                 });
             });

            Task.Factory.StartNew(receive);
        }
    }
}