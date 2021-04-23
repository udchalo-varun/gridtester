using Microsoft.VisualBasic.FileIO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GridTester
{
    public class CsvFileReader : IDisposable
    {
        private TextFieldParser _parser;

        public CsvFileReader(string file)
        {
            _parser = new TextFieldParser(file);
            _parser.TextFieldType = FieldType.Delimited;
            _parser.SetDelimiters(",");
        }

        public bool EndOfData()
        {
            return _parser?.EndOfData ?? true;
        }

        public List<string> ReadLine()
        {
            if (!(_parser?.EndOfData ?? true))
            {
                //Process row
                return _parser.ReadFields().ToList();
            }

            return null;
        }

        public void Close()
        {
            if (_parser != null)
            {
                _parser.Close();
            }
            _parser = null;
        }

        public void Dispose()
        {
            if (_parser != null)
                Close();
        }
    }
}
