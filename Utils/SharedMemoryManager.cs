using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using SharedMemory;

namespace Utils
{
    public class SharedMemoryManager
    {
        private const int InitialBufferSize = 1024;
        private static SharedMemoryManager _instance;
        private static readonly object locker = new object();
        private readonly IDictionary<string, BufferReadWrite> _bufferDictionary;

        private SharedMemoryManager()
        {
            _bufferDictionary = new Dictionary<string, BufferReadWrite>();
        }

        public static SharedMemoryManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (locker)
                    {
                        if (_instance == null)
                        {
                            _instance = new SharedMemoryManager();
                        }
                    }
                }
                return _instance;
            }
        }

        public string Read(string name)
        {
            TryBuffer(name);
            var lengthBytes = new byte[4];
            _bufferDictionary[name].Read(lengthBytes);
            var length = BitConverter.ToInt32(lengthBytes, 0);
            var valueBytes = new byte[length];
            _bufferDictionary[name].Read(valueBytes, 4);
            var valueString = Encoding.ASCII.GetString(valueBytes);
            return valueString;
        }

        public void Write(string name, string value)
        {
            TryBuffer(name);
            var valueBytes = Encoding.ASCII.GetBytes(value);
            var lengthBytes = BitConverter.GetBytes(valueBytes.Length);
            var mergedBytes = lengthBytes.Concat(valueBytes).ToArray();
            _bufferDictionary[name].Write(mergedBytes);
        }

        public T Read<T>(string name) where T : class
        {
            TryBuffer(name);
            var lengthBytes = new byte[4];
            _bufferDictionary[name].Read(lengthBytes);
            var length = BitConverter.ToInt32(lengthBytes, 0);
            var valueBytes = new byte[length];
            _bufferDictionary[name].Read(valueBytes, 4);
            var valueString = Encoding.ASCII.GetString(valueBytes);
            var data = JsonConvert.DeserializeObject(valueString, typeof(T)) as T;
            return data;
        }

        public void Write<T>(string name, T value) where T : class
        {
            TryBuffer(name);
            var valueBytes = Encoding.ASCII.GetBytes(JsonConvert.SerializeObject(value));
            var lengthBytes = BitConverter.GetBytes(valueBytes.Length);
            var mergedBytes = lengthBytes.Concat(valueBytes).ToArray();
            _bufferDictionary[name].Write(mergedBytes);
        }

        private void TryBuffer(string name)
        {
            try
            {
                if (!_bufferDictionary.ContainsKey(name))
                {
                    _bufferDictionary.Add(name, new BufferReadWrite(name, InitialBufferSize));
                }
            }
            catch
            {
                _bufferDictionary.Add(name, new BufferReadWrite(name));
            }
        }
    }
}