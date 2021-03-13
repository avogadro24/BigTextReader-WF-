using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Reflection;
using System.Collections.Concurrent;


namespace BigTextReader_WF_
{

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
            
        }

        private void Form1_Doc(object sender, EventArgs e)
        {
            
        }



        private void button1_OpenFile(object sender, EventArgs e)
        {
            OpenFile();
        }

        private void button2_CountWords(object sender, EventArgs e)
        {
            ButtonEvent();
        }



        void OpenFile()
        {

            OpenFileDialog myDialog = new OpenFileDialog();
            myDialog.Filter = ".txt|*.txt|.fb2|*.fb2";
            if (myDialog.ShowDialog() == DialogResult.OK)
            {
                FilePath = myDialog.FileName;//GET FILE PATH 
                textBoxPath.Text = FilePath;
            }
        }







        string FilePath;
        string ReadFile_StreamReader_ToString(string filePath)
        {
            List<string> Words = new List<string>();

            if (filePath != null)
            {

                using (StreamReader sr = new StreamReader(filePath))
                {

                    string line = string.Empty;

                    while ((line = sr.ReadLine()) != null)
                    {
                        Words.Add(line);
                    }
                }

            }
            return string.Join(" ", Words.ToArray());
        }


        string inputStr;
        string modifietSrting;
        string StringTransform(string inputSrtring)
        {

            inputSrtring = inputSrtring.ToLower();

            Regex reg = new Regex("[^ а-я0-9]");//все не буквы и цыфры
            inputSrtring = reg.Replace(inputSrtring, " ");//заменяем на пробелы

            return inputSrtring;
        }


        //СЛОВАРЬ ДЛЯ ОДНОПОТОЧНОГО МЕТОДА
        Dictionary<string, int> CountTheOccurrences;

        void CallMyPrivateDictionary()
        {
            string assPath= Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);//получить путь 

            Assembly myAssembly = Assembly.LoadFile(assPath+@"\ClassLibrary1.dll");// подключение библиотеки
            Type calcType = myAssembly.GetType("ClassLibrary1.Class1");//получить тип класса
            object calcInstance = Activator.CreateInstance(calcType);//создать экземпляр


            var TEST = calcType.GetMethod("ReturnDict", BindingFlags.Instance | BindingFlags.NonPublic);
            if (TEST != null)
            {
                CountTheOccurrences = (Dictionary<string, int>)TEST.Invoke(calcInstance, new object[] { modifietSrting });
            }
        }

        //СЛОВАРЬ ДЛЯ МНОГОПОТОЧНОГО МЕТОДА
        ConcurrentDictionary<string, int> ParallelCountTheOccurrences;

        void CallMyPublicDictionary()
        {
            string assPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);//получить путь 

            Assembly myAssembly = Assembly.LoadFile(assPath + @"\ClassLibrary1.dll");// подключение библиотеки
            Type calcType = myAssembly.GetType("ClassLibrary1.Class1");//получить тип класса
            object calcInstance = Activator.CreateInstance(calcType);//создать экземпляр


            var TEST = calcType.GetMethod("ReturnDictPublic", BindingFlags.Instance | BindingFlags.Public);
            if (TEST != null)
            {
                ParallelCountTheOccurrences = (ConcurrentDictionary<string, int>)TEST.Invoke(calcInstance, new object[] { modifietSrting });
            }
        }


        class WordsList// класс для сортировки словаря
        {
            public string NameWords;
            public int CountWords;
        }
        class NameComparer : IComparer<WordsList> //сортировщик
        {
            public int Compare(WordsList o1, WordsList o2)
            {
                if (o1.CountWords > o2.CountWords)
                {
                    return -1;
                }
                else if (o1.CountWords < o2.CountWords)
                {
                    return 1;
                }

                return 0;
            }
        }



        NameComparer nameComparer = new NameComparer();
        List<WordsList> MyList = new List<WordsList>();//создаем лист наших классов
        void DictionaryToClass()
        {

            foreach (KeyValuePair<string, int> kvp in ParallelCountTheOccurrences)//прописываем в них значения из словаря
            {

                MyList.Add(new WordsList() { NameWords = kvp.Key, CountWords = kvp.Value });

            }

            MyList.Sort(nameComparer);//сортируем лист 
        }



        void InputFile()
        {
            string assPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string path = assPath+ @"\TestResult.txt"; //System.Reflection.Assembly.GetExecutingAssembly().Location;
            using (StreamWriter w = File.CreateText(path))
            {
                foreach (var item in MyList)
                {
                    w.Write($"{item.NameWords } повторяется { item.CountWords} раз.{Environment.NewLine}");
                }

            }

            Process.Start(path);
        }



        void ButtonEvent()
        {

            //  извлеч текст в строку
            inputStr = ReadFile_StreamReader_ToString(FilePath);

            //  преобразовать текст в нижний регистр , убратьлишние знаки
            modifietSrting = StringTransform(inputStr);




            // CountMethodTime(CallMyPrivateDictionary); //однопоточный  (Elapsed):00:00:01.8553234

               CountMethodTime(CallMyPublicDictionary);  //многопоточный (Elapsed):00:00:01.2943234




            //  сортируем словарь
            DictionaryToClass();

            //  вывод списка слов в файл
            InputFile();

        }
        

        private void CountMethodTime(Action action)
        {
            var stwh = new Stopwatch();

            stwh.Start();

            /////////
            action();
            /////////


            stwh.Stop();

 
            TimeSpan ts = stwh.Elapsed;
            textBoxPath.Text = ($"Метод  / Время выполнения (Elapsed):{ stwh.Elapsed}");
            Console.WriteLine($"Метод  / Время выполнения (Elapsed):{ stwh.Elapsed}");
            Console.WriteLine($"Метод  / Время выполнения (TimeSpan):" + string.Format("{0:00}:{1:00}:{2:00}.{3:00}", ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds / 10));

        }

    }
}
