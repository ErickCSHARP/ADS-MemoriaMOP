using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Atv2_Memoria
{
    public class MemoryProcess
    {
        // Constantes de uso nas funcoes do sistema operacional
        public const int PROCESS_VM_WRITE = 0x0020;
        public const int PROCESS_WM_READ = 0x0010;
        public const int PROCESS_VM_OPERATION = 0x0008;
        public const int PROCESS_ALL_ACCESS = 0x1F0FFF;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteProcessMemory(int hProcess, long lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(int hProcess, long lpBaseAddress, byte[] lpBuffer, int dwSize, ref int lpNumberOfBytesRead);
      
        private readonly int _idProcess;
        
        public MemoryProcess(int idProcess)
        {
            _idProcess = idProcess;
        }

        public byte[] Read(long address, int size)
        {
            // recebe um identificador de manipulaçao
            IntPtr processHandle = OpenProcess(PROCESS_WM_READ, false, _idProcess);


            int bytesRead = 0;
            byte[] buffer = new byte[size];

            // chama funçao do sistema operacional para ler os bytes do endereço de memoria
            ReadProcessMemory((int)processHandle, address, buffer, buffer.Length, ref bytesRead);

            if (bytesRead != size)
            {
                throw new Exception($"Nao foi possivel ler a quantidade de bytes especificada  :  {bytesRead} de {size}] lidos");
            }

            return buffer;
        }

        public void Write(long address, byte[] bytes)
        {
            // recebe um identificador de manipulaçao
            IntPtr processHandle = OpenProcess(PROCESS_ALL_ACCESS, false, _idProcess);

            int bytesWritten = 0;

            // chama funçao do sistema operacional para escrever o array de bytes a partir do endereço de memoria
            WriteProcessMemory((int)processHandle, address, bytes, bytes.Length, ref bytesWritten);

            if (bytesWritten != bytes.Count())
            {
                throw new Exception($"Nao foi possivel escrever a quantidade de bytes total na memoria  :  {bytesWritten} de {bytes.Count()}] escritos");
            }
        }

    
        public void WriteAsString(long address, string text, Encoding encoder)
        {
            var bytes = encoder.GetBytes(text);

            Write(address, bytes);
        }

        public string ReadAsString(long address, int size, Encoding encoder)
        {
            var bytes = Read(address, size);

            var str = encoder.GetString(bytes);

            return str;
        }

        public long ReadAsAddress(long address)
        {
            var bytes = Read(address, 8);

            long pointer = BitConverter.ToInt64(bytes);

            return pointer;
        }

     
    }
}
