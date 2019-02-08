// Code from GoodTimeStudio/Minecraft-Server-Status-Checker/ServerPinger/
// https://github.com/GoodTimeStudio/Minecraft-Server-Status-Checker/tree/master/ServerPinger
// 

// The MIT License (MIT)

// Copyright(c) 2015 GoodTime Studio

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using GammaLibrary.Extensions;
using Newtonsoft.Json;

namespace GoodTimeStudio.ServerPinger
{
    public class ServerPinger
    {
        
        public static Task<ServerStatus> GetStatus(string ServerAddress,
            int? ServerPort = null)
        {
            var split = ServerAddress.Split(':');
            string address;
            int port;
            if (split.Length == 2)
            {
                address = split[0];
                port = split[1].ToInt();
            }
            else
            {
                address = ServerAddress;
                port = ServerPort ?? 25565;
            }
            return GetStatusCurrent(address, port);
        }

        private static async Task<ServerStatus> GetStatusCurrent(string ServerAddress, int ServerPort)
        {
            try
            {
                using (var socket = new TcpClient())
                {
                    socket.SendTimeout = 3000;
                    socket.ReceiveTimeout = 3000;
                    socket.ConnectAsync(ServerAddress, ServerPort).Wait(3000);
                    BinaryWriter writer;

                    #region handshake

                    var handshakeStream = new MemoryStream();
                    var handshakewriter = new BinaryWriter(handshakeStream);

                    handshakewriter.Write((byte)0x00); // Packet ID
                    // Protocol version, http://wiki.vg/Protocol_version_numbers
                    handshakewriter.Write(VarintHelper.IntToVarint(210));
                    handshakewriter.Write(GetByteFromString(ServerAddress)); // hostname or IP
                    handshakewriter.Write((short)ServerPort); // Port
                    handshakewriter.Write(VarintHelper.IntToVarint(0x01)); // Next state, 1 for `status'
                    handshakewriter.Flush();

                    writer = new BinaryWriter(socket.GetStream());
                    writer.Write(VarintHelper.IntToVarint((int)handshakeStream.Length));
                    writer.Write(handshakeStream.ToArray());
                    writer.Flush();

                    #endregion

                    writer = new BinaryWriter(socket.GetStream());
                    /* BE: 0x0100, Length and writer.Write((byte)0x00);
                     * ID for `Request'
                     */
                    writer.Write((short)0x0001);
                    writer.Flush();
                    var streamIn = socket.GetStream();
                    var reader = new BinaryReader(streamIn);
                    var packetLen = VarintHelper.ReadVarInt(reader);
                    var packetId = VarintHelper.ReadVarInt(reader);
                    var packetJsonLen = VarintHelper.ReadVarInt(reader);
                    var response = reader.ReadBytes(packetJsonLen);
                    var json = Encoding.UTF8.GetString(response);
                    Console.WriteLine(json);
                    return JsonConvert.DeserializeObject<ServerStatus>(json);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }

        private static byte[] GetByteFromString(string content)
        {
            var output = new List<byte>();

            output.AddRange(VarintHelper.IntToVarint(content.Length));
            output.AddRange(Encoding.UTF8.GetBytes(content));

            return output.ToArray();
        }
    }
}