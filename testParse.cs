using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace assigment1
{
    public class testParse
    {
        string term, nextWord, Text, doc;
        char number;
        string DocNo, DATE1, Title;
        int flag, Index, count, currentPhase, docLength, bastardo, DoCounTerm, month, dateMon;
        public string tarPath = "";
        public Dictionary<string, List<string[]>> DictC;

        Dictionary<string, bool> stopDic, tmpDOC;
        public Dictionary<string, bool> stopGigDic;
        public Dictionary<string, KeyValuePair<int[], string>> Uniques;
        bool newDoc, between;
        public bool stemmer = false;
        bool makaf = false;
        public int counter = 0;
        public int avgdl = 0;
        public Dictionary<string, KeyValuePair<string, int>> Occurences;
        Stemmer st = new Stemmer();
        public Dictionary<string, int> OneTimeOcc;

        public testParse()
        {

            DictC = new Dictionary<string, List<string[]>>();
            Occurences = new Dictionary<string, KeyValuePair<string, int>>();
            OneTimeOcc = new Dictionary<string, int>();
            Uniques = new Dictionary<string, KeyValuePair<int[], string>>();
            Text = "";
            DocNo = "";
            DATE1 = "";
            Title = "";
            Index = 0;
            flag = 0;
            term = "";
            nextWord = "";
            currentPhase = 0;
            bastardo = 0;

        }

        public void startP(string doc)
        {
            DictC = new Dictionary<string, List<string[]>>();
            tmpDOC = new Dictionary<string, bool>();
            Text = "";
            DocNo = "";
            DATE1 = "";
            Title = "";
            this.doc = doc;
            docLength = doc.Length;
            Index = 0;
            flag = 0;
            term = "";
            nextWord = "";
            bastardo = 0;
            between = false;
            DoCounTerm = 0;
            parse();

        }

        public void parse()
        {

            while (findPhase() != 5 && currentPhase != 5)
            {

                switch (currentPhase)
                {
                    case 1:
                        DocNo = getPhase();
                        Index += 8;
                        break;
                    case 2:
                        DATE1 = getPhase();
                        string[] s = DATE1.Split(' ');
                        if (s.Length > 1 && isMonth(s[1]))
                        {
                            dateMon = month;
                        }
                        Index += 8;
                        break;
                    case 3:
                        Title = getPhase();
                        Index += 5;
                        break;
                    case 4:
                        StartParse();
                        Index += 7;
                        break;
                    case 6:
                        continueParse();
                        break;

                }
            }
            if (tmpDOC.Count > 0)
            {
                Uniques.Add(DocNo, new KeyValuePair<int[], string>(new int[] { tmpDOC.Count, DoCounTerm, dateMon }, Title));
                avgdl += DoCounTerm;
            }
        }

        private void continueParse()
        {
            while (Index < doc.Length && doc[Index] != '<')
            {
                char ccc = doc[Index];
                Index++;
            }
            if (Index == doc.Length)
            {
                currentPhase = 5;
            }
        }

        private string getPhase()
        {
            string word = "";
            if (currentPhase == 2)
            {
                while (Index < doc.Length && doc[Index] != '<')
                {
                    
                    
                    if (checkChar())
                    {
                        if (word == "" && doc[Index] == ' ')
                        {

                        }
                        else
                        {
                            word += doc[Index];
                        }

                    }

                    Index++;
                }
            }
            else
            {
                while (Index < doc.Length && doc[Index] != '<')
                {
                    if (checkChar())
                        word += doc[Index];
                    Index++;
                }
                
            }
            

            return word;
        }

        private void StartParse()
        {

            while (Index < doc.Length)
            {

                if (doc[Index - 1] == '<' && doc[Index] == '/' && doc[Index + 1] == 'T')
                {
                    break;
                }

                if (getNextWord())
                    checkWord(nextWord);
            }
        }

        private int findPhase()
        {
            int phase = 0;
            while (Index < doc.Length)
            {
                string pWord = getWord2();
                switch (pWord)
                {
                    case "<DOCNO>":
                        newDoc = true;
                        currentPhase = 1;
                        counter++;
                        if (DocNo != "")
                        {
                            Uniques.Add(DocNo, new KeyValuePair<int[], string>(new int[] { tmpDOC.Count, DoCounTerm, dateMon }, Title));
                            avgdl += DoCounTerm;
                        }
                        tmpDOC = new Dictionary<string, bool>();
                        DoCounTerm = 0;

                        return 1;
                    case "<DATE1>":
                        currentPhase = 2;
                        return 2;
                    case "<TI>":
                        currentPhase = 3;
                        return 3;
                    case "<TEXT>":
                        flag = 0;
                        bastardo = 0;
                        currentPhase = 4;
                        return 4;
                    default:
                        currentPhase = 6;
                        return 6;

                }
            }
            if (Index > doc.Length)
            {
                phase = 5;
            }
            return phase;
        }

        private string getWord2()
        {
            string word = "";
        label:
            do
            {
                if (checkChar2())
                {
                    word += doc[Index];
                    if (word[0] != '<')
                    {
                        return "";
                    }
                }
                Index++;
            }
            while (Index < doc.Length && doc[Index - 1] != '>');

            //Index++;
            if (Index < doc.Length && word == "")
                goto label;
            return word;
        }

        private void Chaos(string term, int place, string DocNo, string DATE1, string Title)
        {
            int fIdx = 0;
            int tIdx = 0;
            if (term == "")
            {
                return;
            }
            if (stemmer)
            {
                term = st.stemTerm(term);
            }
            if (term.Any(c => char.IsUpper(c)))
            {
                term = term.Trim().ToLower();
            }
            DoCounTerm++;

            if (!tmpDOC.ContainsKey(term))
                tmpDOC.Add(term, true);
            
            if (DictC.ContainsKey(term))
            {
                int check = DocNo.CompareTo(DictC[term][DictC[term].Count - 1][0]);
                int cntD = DictC[term].Count;
                if (check == 0)
                {
                    OneTimeOcc[DocNo]--;
                    DictC[term][DictC[term].Count - 1][1] += "|" + place.ToString();
                    DictC[term][DictC[term].Count - 1][2] = cntD.ToString();
                    int tmp = Int32.Parse(DictC[term][DictC[term].Count - 1][3]);
                    tmp++;
                    DictC[term][DictC[term].Count - 1][3] = tmp.ToString();
                    int tmp2 = Int32.Parse(DictC[term][DictC[term].Count - 1][5]);
                    tmp2 += place;
                    DictC[term][DictC[term].Count - 1][5] = tmp2.ToString();
                    if (Occurences.ContainsKey(DocNo))
                    {
                        if (Occurences[DocNo].Value < tmp)
                        {
                            Occurences.Remove(DocNo);
                            Occurences.Add(DocNo, new KeyValuePair<string, int>(term, tmp));
                        }
                    }
                    else
                        Occurences.Add(DocNo, new KeyValuePair<string, int>(term, tmp));
                }
                else
                {
                    if (OneTimeOcc.ContainsKey(DocNo))
                        OneTimeOcc[DocNo]++;
                    else
                        OneTimeOcc.Add(DocNo, 1);
                    cntD++;
                    int cntO = 1;
                    tIdx += place;
                    fIdx = place;
                    DictC[term].Add(new string[] { DocNo, place.ToString(), cntD.ToString(), cntO.ToString(), fIdx.ToString(), tIdx.ToString() });
                    if (Occurences.ContainsKey(DocNo))
                    {
                        if (Occurences[DocNo].Value < cntO)
                        {
                            Occurences.Remove(DocNo);
                            Occurences.Add(DocNo, new KeyValuePair<string, int>(term, cntO));
                        }
                    }
                    else
                        Occurences.Add(DocNo, new KeyValuePair<string, int>(term, cntO));
                }
            }
            else
            {
                if (OneTimeOcc.ContainsKey(DocNo))
                    OneTimeOcc[DocNo]++;
                else
                    OneTimeOcc.Add(DocNo, 1);
                DictC.Add(term, new List<string[]>());
                int cntD = DictC[term].Count + 1;
                int cntO = 1;
                tIdx += place;
                fIdx = place;
                DictC[term].Add(new string[] { DocNo, place.ToString(), cntD.ToString(), cntO.ToString(), fIdx.ToString(), tIdx.ToString() });
                if (Occurences.ContainsKey(DocNo))
                {
                    if (Occurences[DocNo].Value < cntO)
                    {
                        Occurences.Remove(DocNo);
                        Occurences.Add(DocNo, new KeyValuePair<string, int>(term, cntO));
                    }
                }
                else
                    Occurences.Add(DocNo, new KeyValuePair<string, int>(term, cntO));
            }
        }

        private void checkWord(string word)
        {
            int n;

            if (flag == 0) // new term
            {
                if (int.TryParse(word[0] + "", out n))
                {
                    //term += word;
                    checkNumeric(word);
                }
                else
                {
                    checkNonNumeric(word);
                }
            }
            else if (flag == 1) // num + fraction
            {

                if (isNumber(word) || isKg(word) || isPercent(word))
                {
                    term += " " + word;
                    InsertDic();
                }
                else
                {
                    InsertDic(bastardo - (word.Length + 1));
                    flag = 0;
                    checkWord(word);
                }
            }
            else if (flag == 2) // dollars ##
            {
                if (word[word.Length - 1] != '.')
                {
                    if (getNextWord())
                    {
                        if (int.TryParse(word[0] + "", out n) && nextWord.Contains('/'))
                        {
                            term += " " + nextWord;
                            InsertDic();
                        }
                        else
                        {
                            InsertDic(bastardo - (word.Length + 1));
                            flag = 0;
                            checkWord(nextWord);
                        }
                    }
                    else
                    {
                        InsertDic();
                    }
                }
                else
                {
                    flag = 0;
                    term = term.Substring(0, term.Length - 1);
                    InsertDic();
                }
            }
            else if (flag == 3) // capital letter
            {
                if (char.IsUpper(word[0]))
                    checkUpper(word);
                else
                {
                    InsertDic(bastardo - (word.Length + 1));
                    flag = 0;
                    checkWord(word);
                }
            }
            else if (flag == 4) // $
            {
                bool endSen = false;

                if (word[word.Length - 1] == '.')
                {
                    nextWord = nextWord.Substring(0, nextWord.Length - 1);
                    endSen = true;
                }

                else if (int.TryParse(word[0] + "", out n) && word.Contains('/'))
                {
                    flag = 1;
                    term += " " + word;
                    if (getNextWord())
                        checkWord(nextWord);
                    else
                    {
                        InsertDic();
                    }
                }
                else
                {
                    flag = 0;
                    InsertDic();
                    checkWord(word);
                }
            }
            else if (flag == 5) // ## month
            {
                bool endSen = false;
                if (word[word.Length - 1] == '.')
                {
                    word = word.Substring(0, word.Length - 1);
                    endSen = true;
                }
                if (int.TryParse(word, out n))
                {
                    if (word.Length == 2)
                        term += ".19" + word;
                    else
                        term += '.' + word;
                    flag = 0;
                    InsertDic();
                }
                else
                {
                    InsertDic(bastardo - (word.Length + 1));
                    flag = 0;
                    if (!endSen)
                        checkWord(word);
                    else
                    {
                        term = word;
                        InsertDic();
                    }

                }

            }
            else if (flag == 6) // month ##
            {
                if (getNextWord())
                {
                    bool endSen = false;
                    if (nextWord[nextWord.Length - 1] == '.')
                    {
                        nextWord = nextWord.Substring(0, nextWord.Length - 1);
                        endSen = true;
                    }

                    if (int.TryParse(nextWord, out n))
                    {
                        if (nextWord.Length > 2)
                            term += '.' + nextWord;
                        else
                            term += ".19" + nextWord;
                        flag = 0;
                        InsertDic();
                    }
                    else
                    {
                        InsertDic(bastardo - (nextWord.Length + 1));
                        flag = 0;
                        if (!endSen)
                            checkWord(nextWord);
                        else
                        {
                            term += nextWord;
                            InsertDic();
                        }

                    }
                }
                else
                {
                    InsertDic();
                }
            }

        }

        private void checkNonNumeric(string word)
        {
            if (!between)
            {
                char c = word[0];

                if (char.IsUpper(c))
                {
                    checkUpper(word);
                }
                else if (c == '$')
                {
                    if (word.Length > 1)
                    {
                        hendledollarsign(word.Substring(1));
                    }

                    //if (word[word.Length - 1] != '.')
                    //{
                    //    flag = 4;
                    //    term += word;
                    //    if (getNextWord())
                    //        checkWord(nextWord);
                    //    else InsertDic();
                    //}
                    //else
                    //{
                    //    term = word.Substring(0, word.Length - 1);
                    //    InsertDic();
                    //}
                }
                else
                {
                    if (word[word.Length - 1] == '.')
                        word = word.Substring(0, word.Length - 1);
                    flag = 0;
                    term = word;
                    InsertDic();

                }
            }
            else
            {
                if (getNextWord())
                {
                    int n;
                    if (Int32.TryParse(nextWord, out n))
                    {
                        if (flag != 0)
                            InsertDic();
                        flag = 0;
                        between = true;
                        term = word + " " + nextWord;
                        getNextWord();
                        term += " " + nextWord;
                        getNextWord();
                        term += " " + nextWord;
                        between = false;
                        InsertDic();
                    }
                    else
                    {
                        between = false;
                        checkWord(nextWord);
                    }
                }
            }
        }

        private void InsertDic()
        {
            //DictA.Add(Index, term);
            Chaos(term, bastardo, DocNo, DATE1, Title);
            term = "";
        }

        private double TreateFraction(string word)
        {
            double num = -1;
            string num1, num2;
            int backslash = word.IndexOf('/');
            num1 = word.Substring(0, backslash);
            double n1;
            bool isNumeric1 = Double.TryParse(num1, out n1);
            if (isNumeric1)
            {
                num2 = word.Substring(backslash + 1);
                double n2;
                bool isNumeric2 = Double.TryParse(num2, out n2);
                if (isNumeric2 && n2 != 0)
                {
                    num = n1 / n2;
                }
            }
            return num;
        }

        private int TreateNumber(string word)
        {
            double num = 0;
            switch (number)
            {
                case 'm':
                    num = Double.Parse(word) * 1000000;
                    break;
                case 't':
                    num = Double.Parse(word) * 10000000;
                    break;
                case 'h':
                    num = Double.Parse(word) * 100;
                    break;
                case 'd':
                    num = Double.Parse(word) * 1000;
                    break;
            }

            return (int)num;
        }

        private void InsertDic(int place)
        {
            //DictA.Add(place, term);
            Chaos(term, place, DocNo, DATE1, Title);
            term = "";
        }

        private void checkUpper(string word)
        {
            if (!between)
            {
                int n;

                if (word[word.Length - 1] != '.')
                {
                    if (getNextWord())
                    {
                        string justMonth = nextWord;
                        bool endSen = false;
                        if (nextWord[nextWord.Length - 1] == '.')
                        {
                            justMonth = nextWord.Substring(0, nextWord.Length - 1);
                            endSen = true;
                        }
                        if (word == "Dollars")
                        {

                            if (checkDollar(nextWord))
                            {
                                if (flag != 0)
                                    InsertDic(bastardo - (nextWord.Length + 1));

                                term += "dollars" + ' ';
                                hendledollar(nextWord);
                                flag = 0;
                                //term += word + " " + nextWord;
                                //flag = 2;
                                //checkWord(nextWord);
                            }
                            else
                            {
                                if (flag == 0)
                                    term = Char.ToLower(word[0]) + word.Substring(1);
                                else
                                    term += " " + Char.ToLower(word[0]) + word.Substring(1);
                                flag = 3;
                                checkWord(nextWord);

                            }
                        }
                        else if (isMonth(word) && int.TryParse(justMonth, out n))
                        {
                            if (flag != 0)
                                InsertDic(bastardo - (nextWord.Length + 1));
                            if (!endSen)
                            {
                                if (justMonth.Length > 2)
                                {
                                    flag = 0;
                                    term = month.ToString() + '.' + justMonth;
                                    InsertDic();
                                }
                                else
                                {
                                    if (Int32.Parse(justMonth) <= 31)
                                    {
                                        flag = 6;
                                        term = justMonth + '.' + month;
                                        checkWord(justMonth);
                                    }
                                    else
                                    {
                                        flag = 0;
                                        term += month + ".19" + justMonth;
                                        InsertDic();
                                    }
                                }
                            }
                            else
                            {
                                flag = 0;
                                if (justMonth.Length > 2)
                                {
                                    term = month.ToString() + '.' + justMonth;
                                    InsertDic();
                                }
                                else
                                {
                                    if (Int32.Parse(justMonth) <= 31)
                                    {
                                        term = justMonth + '.' + month;
                                        InsertDic();
                                    }
                                    else
                                    {
                                        term += month + ".19" + justMonth;
                                        InsertDic();
                                    }
                                }

                            }
                        }
                        else
                        {
                            if (flag == 0)
                                term = word;
                            else
                                term += " " + word;
                            flag = 3;
                            checkWord(nextWord);
                        }
                    }
                    else
                    {
                        if (flag != 0)
                            term += " " + word;
                        else
                            term = word;
                        flag = 0;

                        InsertDic();
                    }
                }
                else
                {
                    if (flag == 0)
                    {
                        word = word.Substring(0, word.Length - 1);
                        term = word;
                    }
                    else
                    {
                        word = word.Substring(0, word.Length - 1);
                        term += " " + word;
                    }
                    flag = 0;
                    InsertDic();
                }
            }
            else
            {
                if (getNextWord())
                {
                    int n;
                    if (Int32.TryParse(nextWord, out n))
                    {
                        if (flag != 0)
                            InsertDic();
                        flag = 0;
                        between = true;
                        string s = "";
                        s = nextWord + '-';
                        getNextWord();
                        getNextWord();
                        s += nextWord;
                        between = false;
                        checkWord(s);
                    }
                    else
                    {
                        between = false;
                        checkWord(nextWord);
                    }
                }
                else
                {
                    term += word;
                    InsertDic();
                }
            }
        }

        private void hendledollar(string word)
        {
            double n;

            if (word[word.Length - 1] != '.')
            {
                if (word.Contains('-'))
                {
                    int num1;

                    int mak = word.IndexOf('-');
                    if (Int32.TryParse(word.Substring(0, mak), out num1))
                    {
                        char tmp = word[word.Length - 1];
                        bool numbers = false;
                        if (tmp == 'n')
                        {
                            word = word.Substring(0, word.Length - 2);
                            isNumber("trillion");
                            numbers = true;
                        }
                        else if (tmp == 'm')
                        {
                            word = word.Substring(0, word.Length - 1);
                            isNumber("million");
                            numbers = true;
                        }
                        int num2;
                        string s = word.Substring(mak + 1);
                        if (Int32.TryParse(word.Substring(mak + 1), out num2))
                        {
                            if (numbers)
                            {
                                flag = 0;
                                int fNum1 = TreateNumber(num1 + "");
                                int fNum2 = TreateNumber(num2 + "");
                                term += fNum1.ToString() + '-' + fNum2.ToString();
                                InsertDic();
                            }
                            else
                            {
                                flag = 0;
                                term += word;
                                InsertDic();
                            }

                        }
                        else
                        {
                            flag = 0;
                            term += word;
                            InsertDic();
                        }
                    }
                    else
                    {
                        flag = 0;
                        term += word;
                        InsertDic();
                    }
                }
                else if (word.Contains('/'))
                {
                    bool numbers = false;
                    char tmp = word[word.Length - 1];
                    if (tmp == 'n')
                    {
                        word = word.Substring(0, word.Length - 2);
                        numbers = true;
                    }
                    else if (tmp == 'm')
                    {
                        word = word.Substring(0, word.Length - 1);
                        numbers = true;
                    }
                    double num = TreateFraction(word);
                    if (num != -1)
                    {
                        bool endSen = false;
                        if (nextWord[nextWord.Length - 1] == '.')
                        {
                            nextWord = nextWord.Substring(0, nextWord.Length - 1);
                            endSen = true;
                        }

                        if (numbers)
                        {
                            if (tmp == 'm')
                            {
                                isNumber("million");
                            }
                            else
                            {
                                isNumber("trillion");
                            }
                            flag = 0;
                            int fullnum = TreateNumber(num + "");
                            term += fullnum.ToString();
                            InsertDic();
                        }
                        else
                        {
                            flag = 0;
                            term += num.ToString();
                            InsertDic();
                        }

                    }
                    else
                    {
                        flag = 0;
                        term += word;
                        InsertDic();
                    }
                }
                else
                {
                    bool pure = Double.TryParse(word, out n);
                    bool endSen = false;
                    if (getNextWord())
                    {
                        if (nextWord[nextWord.Length - 1] == '.')
                        {
                            nextWord = nextWord.Substring(0, nextWord.Length - 1);
                            endSen = true;
                        }
                        bool numbers = false;
                        char tmp = word[word.Length - 1];
                        if (tmp == 'n')
                        {
                            word = word.Substring(0, word.Length - 2);
                            numbers = true;
                        }
                        else if (tmp == 'm')
                        {
                            word = word.Substring(0, word.Length - 1);
                            numbers = true;
                        }
                        if (pure && nextWord.Contains('/'))
                        {
                            bool numbers2 = false;
                            char tmp2 = nextWord[nextWord.Length - 1];
                            if (tmp2 == 'n')
                            {
                                nextWord = nextWord.Substring(0, nextWord.Length - 2);
                                numbers2 = true;
                                number = 't';
                            }
                            else if (tmp2 == 'm')
                            {
                                nextWord = nextWord.Substring(0, nextWord.Length - 1);
                                numbers2 = true;
                                number = 'm';
                            }
                            double num = 0;
                            string num1, num2;
                            int backslash = nextWord.IndexOf('/');
                            num1 = nextWord.Substring(0, backslash);
                            double n1;
                            bool isNumeric1 = Double.TryParse(num1, out n1);
                            if (isNumeric1)
                            {
                                num2 = nextWord.Substring(backslash + 1);
                                double n2;
                                bool isNumeric2 = Double.TryParse(num2, out n2);
                                if (isNumeric2 && n2 != 0)
                                {
                                    double fraction = n1 / n2;
                                    num += Double.Parse(word) + fraction;

                                    if (numbers2)
                                        num = TreateNumber(num.ToString());
                                    if (!endSen)
                                        hendledollar(num.ToString());
                                    else
                                    {
                                        flag = 0;
                                        term += num.ToString();
                                        InsertDic();
                                    }
                                }
                                else
                                {
                                    flag = 0;
                                    term = word;
                                    InsertDic(bastardo - (nextWord.Length + 1));
                                    term = nextWord;
                                    InsertDic();
                                }
                            }
                            else
                            {
                                flag = 0;
                                term = word;
                                InsertDic(bastardo - (nextWord.Length + 1));
                                term = nextWord;
                                InsertDic();
                            }
                        }
                        else if (numbers)
                        {
                            if (tmp == 'm')
                            {
                                isNumber("million");

                            }
                            else
                            {
                                isNumber("trillion");
                            }
                            flag = 0;
                            int fullnum = TreateNumber(word);
                            term += fullnum.ToString();
                            InsertDic();
                        }

                    }
                    else
                    {
                        char tmp = word[word.Length - 1];
                        if (tmp == 'n')
                        {
                            flag = 0;
                            isNumber("trillion");
                            word = word.Substring(0, word.Length - 2);
                            int fullnum = TreateNumber(word);
                            term += fullnum.ToString();
                            InsertDic();
                        }
                        else if (tmp == 'm')
                        {
                            flag = 0;
                            isNumber("million");
                            word = word.Substring(0, word.Length - 1);
                            int fullnum = TreateNumber(word);
                            term += fullnum.ToString();
                            InsertDic();
                        }
                        else
                        {
                            flag = 0;
                            term += word;
                            InsertDic();
                        }
                    }
                }
            }
            else
            {
                word.Substring(0, word.Length - 1);
                if (word.Contains('/'))
                {
                    double num = TreateFraction(word);
                    if (num != -1)
                    {
                        term += num.ToString();
                    }
                    else
                    {
                        term += word;
                    }
                    flag = 0;
                    InsertDic();
                }
                else
                {
                    term += word;
                    flag = 0;
                    InsertDic();
                }
            }
        }

        private bool checkDollar(string p)
        {
            int n;
            return int.TryParse(p.Substring(0, 1), out n);
        }

        private void hendledollarsign(string word)
        {
            double n;
            if (word[word.Length - 1] != '.')
            {
                if (word.Contains('-'))
                {
                    double num1;
                    int tmp = word.IndexOf('-');
                    if (Double.TryParse(word.Substring(0, tmp), out num1))
                    {
                        double num2;
                        if (Double.TryParse(word.Substring(tmp + 2), out num2))
                        {
                            if (getNextWord())
                            {
                                bool endSen = false;
                                if (nextWord[nextWord.Length - 1] == '.')
                                {
                                    endSen = true;
                                    nextWord = nextWord.Substring(0, nextWord.Length - 1);
                                }
                                if (isNumber(nextWord))
                                {
                                    flag = 0;
                                    int tmp1 = TreateNumber(num1.ToString());
                                    int tmp2 = TreateNumber(num2.ToString());
                                    term += "Dollars " + tmp1.ToString() + '-' + tmp2.ToString();
                                    InsertDic();
                                }
                                else
                                {
                                    term += "Dollars " + num1.ToString() + '-' + num2.ToString();
                                    InsertDic(bastardo - (nextWord.Length + 1));
                                    if (!endSen)
                                        checkWord(nextWord);
                                    else
                                    {
                                        flag = 0;
                                        term += nextWord;
                                        InsertDic();
                                    }
                                }
                            }
                            else
                            {
                                flag = 0;
                                term += "Dollars " + num1.ToString() + '-' + num2.ToString();
                                InsertDic();
                            }
                        }
                        else
                        {
                            flag = 0;
                            term += '$' + word;
                            InsertDic();
                        }
                    }
                    else
                    {
                        flag = 0;
                        term += '$' + word;
                        InsertDic();
                    }
                }
                else if (word.Contains('/'))
                {
                    double num = TreateFraction(word);
                    if (num != -1)
                    {
                        bool endSen = false;
                        if (getNextWord())
                        {
                            if (nextWord[nextWord.Length - 1] == '.')
                            {
                                nextWord = nextWord.Substring(0, nextWord.Length - 1);
                                endSen = true;
                            }
                            if (isNumber(nextWord))
                            {
                                flag = 0;
                                int fullnum = TreateNumber(num + "");
                                term += "Dollars " + fullnum.ToString();
                                InsertDic();
                            }
                            else
                            {
                                term += "Dollars " + num.ToString();
                                InsertDic(bastardo - (nextWord.Length + 1));
                                if (!endSen)
                                {
                                    flag = 0;
                                    checkWord(nextWord);
                                }
                                else
                                {
                                    flag = 0;
                                    term += nextWord;
                                    InsertDic();
                                }
                            }
                        }
                        else
                        {
                            term += "Dollars " + num.ToString();
                            InsertDic();
                        }
                    }
                    else
                    {
                        term += '$' + word;
                        InsertDic();
                    }
                }
                else
                {
                    bool pure = Double.TryParse(word, out n);
                    bool endSen = false;
                    if (getNextWord())
                    {
                        if (nextWord[nextWord.Length - 1] == '.')
                        {
                            nextWord = nextWord.Substring(0, nextWord.Length - 1);
                            endSen = true;
                        }
                        if (pure && nextWord.Contains('/'))
                        {
                            double num = 0;
                            string num1, num2;
                            int backslash = nextWord.IndexOf('/');
                            num1 = nextWord.Substring(0, backslash);
                            double n1;
                            bool isNumeric1 = Double.TryParse(num1, out n1);
                            if (isNumeric1)
                            {
                                num2 = nextWord.Substring(backslash + 1);
                                double n2;
                                bool isNumeric2 = Double.TryParse(num2, out n2);
                                if (isNumeric2 && n2 != 0)
                                {
                                    double fraction = n1 / n2;
                                    num += Double.Parse(word) + fraction;
                                    if (!endSen)
                                        hendledollarsign(num.ToString());
                                    else
                                    {
                                        term += "Dollars " + num.ToString();
                                        InsertDic();
                                    }
                                }
                                else
                                {
                                    flag = 0;
                                    term = '$' + word;
                                    InsertDic(bastardo - (nextWord.Length + 1));
                                    term = nextWord;
                                    InsertDic();
                                }
                            }
                            else
                            {
                                flag = 0;
                                term = '$' + word;
                                InsertDic(bastardo - (nextWord.Length + 1));
                                term = nextWord;
                                InsertDic();
                            }
                        }
                        else if (pure && isNumber(nextWord))
                        {
                            int fullnum = TreateNumber(word);
                            term += "Dollars " + fullnum.ToString();
                            InsertDic();
                        }

                    }
                    else
                    {
                        term += '$' + word;
                        InsertDic();
                    }
                }
            }
            else
            {
                word.Substring(0, word.Length - 1);
                if (word.Contains('/'))
                {
                    double num = TreateFraction(word);
                    if (num != -1)
                    {
                        term += "Dollars " + num.ToString();
                    }
                    else
                    {
                        term += '$' + word;
                    }
                    flag = 0;
                    InsertDic();
                }
                else
                {
                    term += word;
                    flag = 0;
                    InsertDic();
                }
            }
        }

        private void checkNumeric(string word)
        {
            double n;
            if (word[word.Length - 1] != '.')
            {
                if (word.Contains('/'))
                {
                    double num = TreateFraction(word);
                    if (num != -1)
                    {
                        checkNumeric(num + "");
                    }
                    else
                    {
                        flag = 0;
                        term += word;
                        InsertDic();
                    }
                }
                else if (word.Contains('-'))
                {
                    double num1;
                    int tmp = word.IndexOf('-');
                    if (Double.TryParse(word.Substring(0, tmp), out num1))
                    {
                        double num2;
                        if (Double.TryParse(word.Substring(tmp + 1), out num2))
                        {
                            if (getNextWord())
                            {
                                bool endSen = false;
                                if (nextWord[nextWord.Length - 1] == '.')
                                {
                                    endSen = true;
                                    nextWord = nextWord.Substring(0, nextWord.Length - 1);
                                }
                                if (isNumber(nextWord))
                                {
                                    flag = 0;
                                    int tmp1 = TreateNumber(num1.ToString());
                                    int tmp2 = TreateNumber(num2.ToString());
                                    term += tmp1.ToString() + '-' + tmp2.ToString();
                                    InsertDic();
                                }
                                else
                                {
                                    flag = 0;
                                    term += word;
                                    InsertDic(bastardo - (nextWord.Length + 1));
                                    if (!endSen)
                                        checkWord(nextWord);
                                    else
                                    {
                                        flag = 0;
                                        term += nextWord;
                                        InsertDic();
                                    }
                                }
                            }
                            else
                            {
                                flag = 0;
                                term += word;
                                InsertDic();
                            }
                        }
                        else
                        {
                            flag = 0;
                            term += word;
                            InsertDic();
                        }
                    }
                    else
                    {
                        flag = 0;
                        term += word;
                        InsertDic();
                    }
                }
                else
                {
                    bool pure = Double.TryParse(word, out n);
                    bool endSen = false;
                    if (getNextWord())
                    {
                        if (nextWord[nextWord.Length - 1] == '.')
                        {
                            nextWord = nextWord.Substring(0, nextWord.Length - 1);
                            endSen = true;
                        }
                        if ((pure || word[word.Length - 1] == 'h') && isMonth(nextWord))
                        {
                            if (word[word.Length - 1] == 'h')
                            {
                                word = word.Substring(0, word.Length - 2);
                            }
                            if (endSen)
                            {
                                flag = 0;
                                term += word + '.' + month;
                                InsertDic();
                            }
                            else
                            {
                                flag = 5;
                                term += word + '.' + month;
                                if (getNextWord())
                                {
                                    checkWord(nextWord);
                                }
                                else
                                {
                                    flag = 0;
                                    InsertDic();
                                }
                            }

                        }
                        else if (pure && isPercent(nextWord))
                        {
                            flag = 0;
                            term += word + " " + nextWord;
                            InsertDic();
                        }
                        else if (pure && isNumber(nextWord))
                        {
                            int num = TreateNumber(word);

                            flag = 0;
                            term += num;
                            InsertDic();
                        }
                        else if (pure && isKg(nextWord))
                        {
                            flag = 0;
                            term += word + " " + nextWord;
                            InsertDic();
                        }
                        else if (pure && isMeters(nextWord))
                        {
                            flag = 0;
                            term += word + " " + nextWord;
                            InsertDic();
                        }
                        else if (pure && nextWord.Contains('/'))
                        {
                            double num = 0;
                            string num1, num2;
                            int backslash = nextWord.IndexOf('/');
                            num1 = nextWord.Substring(0, backslash);
                            double n1;
                            bool isNumeric1 = Double.TryParse(num1, out n1);
                            if (isNumeric1)
                            {
                                num2 = nextWord.Substring(backslash + 1);
                                double n2;
                                bool isNumeric2 = Double.TryParse(num2, out n2);
                                if (isNumeric2 && n2 != 0)
                                {
                                    double fraction = n1 / n2;
                                    num += Double.Parse(word) + fraction;
                                    if (!endSen)
                                        checkNumeric(num.ToString());
                                    else
                                    {
                                        flag = 0;
                                        term += num.ToString();
                                        InsertDic();
                                    }
                                }
                                else
                                {
                                    flag = 0;
                                    term = word;
                                    InsertDic(bastardo - (nextWord.Length + 1));
                                    term = nextWord;
                                    InsertDic();
                                }
                            }
                            else
                            {
                                flag = 0;
                                term = word;
                                InsertDic(bastardo - (nextWord.Length + 1));
                                term = nextWord;
                                InsertDic();
                            }
                        }
                        else
                        {
                            term += word;
                            flag = 0;
                            InsertDic(bastardo - (word.Length + 1));
                            checkWord(nextWord);
                        }
                    }
                    else
                    {
                        flag = 0;
                        term += word;
                        InsertDic();
                    }
                }
            }
            else
            {
                word = word.Substring(0, word.Length - 1);
                if (word.Contains('/'))
                {
                    double num = TreateFraction(word);
                    if (num != -1)
                    {
                        term += num.ToString();
                    }
                    else
                    {
                        term += word;
                    }
                    flag = 0;
                    InsertDic();
                }
                else
                {
                    term += word;
                    flag = 0;
                    InsertDic();
                }
            }

        }

        private bool isMeters(string nextWord)
        {
            return nextWord == "Meters" || nextWord == "Kms" || nextWord == "Kilometer" || nextWord == "Mils";
        }

        private bool isKg(string nextWord)
        {
            return nextWord == "Kgs" || nextWord == "Grams" || nextWord == "Tons";
        }

        private bool isNumber(string word)
        {
            bool Result = false;
            switch (word)
            {
                case "million":
                    number = 'm';
                    Result = true;
                    break;
                case "m":
                    number = 'm';
                    Result = true;
                    break;
                case "n":
                    number = 't';
                    Result = true;
                    break;

                case "trillion":
                    number = 't';
                    Result = true;
                    break;
                case "hundreds":
                    number = 'h';
                    Result = true;
                    break;
                case "thousands":
                    number = 'd';
                    Result = true;
                    break;

            }

            return Result;
        }

        private bool isPercent(string word)
        {
            bool Result = false;
            switch (word)
            {

                case "Percent":
                    Result = true;
                    break;
                case "percents":
                    Result = true;
                    break;
                case "percent":
                    Result = true;
                    break;
                case "percentage":
                    Result = true;
                    break;
            }

            return Result;
        }

        private string getWord()
        {
            string word = "";
        label:
            while (Index < doc.Length && doc[Index] != ' ' && doc[Index] != '<'
                )
            {
                if (doc[Index - 1] == '<' && doc[Index] == '/' && doc[Index + 1] == 'T')
                    return "";
                if (checkChar())
                {
                    int n;
                    int tmp = word.Length - 1;
                    if (word != "" && doc[Index] == 'O' && Int32.TryParse(word[tmp] + "", out n))
                    {
                        word += '0';
                    }
                    else
                    {
                        word += doc[Index];
                    }

                }
                if (makaf)
                {
                    makaf = false;
                    break;
                }
                Index++;
                bastardo++;
            }

            Index++;
            bastardo++;
            if (word.ToLower() == "between")
            {
                between = true;
            }
            if (!between && stopDic.ContainsKey(word))
            {
                word = "";
                goto label;
            }
            if (word != "" && word[0] == '.')
            {
                int i = 0;
                while (i < word.Length)
                {
                    i++;
                }
                word = word.Substring(i);
                if (word == "")
                    goto label;
            }
            if (Index < doc.Length && word == "" && doc[Index] != '<')
                goto label;
            return word;
        }

        private bool getNextWord()
        {
            string str = "";
            str = getWord();

            if (str != "")
            {
                nextWord = str;
                count = str.Length;
                return true;
            }
            return false;
        }

        private bool checkChar2()
        {
            bool Result = true;

            switch (doc[Index])
            {
                case '\r':
                    Result = false;
                    break;
                case '\n':
                    Result = false;
                    break;
                case '\t':
                    Result = false;
                    break;
                case '(':
                    Result = false;
                    break;
                case ')':
                    Result = false;
                    break;
                case '[':
                    Result = false;
                    break;
                case ']':
                    Result = false;
                    break;
                case '{':
                    Result = false;
                    break;
                case '}':
                    Result = false;
                    break;
                case '?':
                    Result = false;
                    break;
                case ':':
                    Result = false;
                    break;
                case '*':
                    Result = false;
                    break;
                case '"':
                    Result = false;
                    break;
                case ',':
                    Result = false;
                    break;
                case '`':
                    Result = false;
                    break;
            }
            return Result;
        }

        private bool checkChar()
        {
            bool Result = true;

            switch (doc[Index])
            {
                case '\r':
                    Result = false;
                    break;
                case '+':
                    Result = false;
                    break;
                case '=':
                    Result = false;
                    break;
                case '@':
                    Result = false;
                    break;
                case '#':
                    Result = false;
                    break;
                case ';':
                    Result = false;
                    break;
                case '\n':
                    Result = false;
                    break;
                case '\t':
                    Result = false;
                    break;
                case '(':
                    Result = false;
                    break;
                case ')':
                    Result = false;
                    break;
                case '<':
                    Result = false;
                    break;
                case '>':
                    Result = false;
                    break;
                case '[':
                    Result = false;
                    break;
                case ']':
                    Result = false;
                    break;
                case '{':
                    Result = false;
                    break;
                case '}':
                    Result = false;
                    break;
                case '?':
                    Result = false;
                    break;
                case ':':
                    Result = false;
                    break;
                case '*':
                    Result = false;
                    break;
                case '"':
                    Result = false;
                    break;
                case ',':
                    Result = false;
                    break;
                case '�':
                    Result = false;
                    break;
                case '|':
                    Result = false;
                    break;
                case '/':
                    int n;
                    if (Int32.TryParse(doc[Index - 1] + "", out n))
                        Result = true;
                    else
                        Result = false;
                    break;
                case '%':
                    int n3;
                    if (Int32.TryParse(doc[Index - 1] + "", out n3))
                        Result = true;
                    else
                        Result = false;
                    break;
                case '`':
                    Result = false;
                    break;
                case '&':
                    Result = false;
                    break;
                case '!':
                    Result = false;
                    break;
                case '-':
                    int n2;
                    if ((doc[Index + 1] == '$' && Int32.TryParse(doc[Index - 1] + "", out n2)) || Int32.TryParse(doc[Index + 1] + "", out n2))
                        Result = true;
                    else
                    {
                        makaf = true;
                        Result = false;
                    }
                    break;
            }
            return Result;
        }

        private bool isMonth(string trem)
        {
            // add more moth key word //
            bool bol = false;
            if (trem.Length > 3)
                switch (trem)
                {

                    case "January":
                        month = 1;
                        bol = true;
                        break;
                    case "February":
                        month = 2;
                        bol = true;
                        break;
                    case "March":
                        month = 3;
                        bol = true;
                        break;
                    case "April":
                        month = 4;
                        bol = true;
                        break;
                    //case "May":
                    //    bol = true;
                    //    break;
                    case "June":
                        month = 6;
                        bol = true;
                        break;
                    case "July":
                        month = 7;
                        bol = true;
                        break;
                    case "August":
                        month = 8;
                        bol = true;
                        break;
                    case "September":
                        month = 9;
                        bol = true;
                        break;
                    case "October":
                        month = 10;
                        bol = true;
                        break;
                    case "November":
                        month = 11;
                        bol = true;
                        break;
                    case "December":
                        month = 12;
                        bol = true;
                        break;

                }
            else
            {
                //Console.WriteLine("in");
                if (trem.Length == 3)
                {
                    char[] arr = new char[3];
                    arr[0] = trem[0];
                    arr[1] = char.ToLower(trem[1]);
                    arr[2] = char.ToLower(trem[2]);
                    trem = new string(arr);
                }
                switch (trem)
                {

                    case "Jan":
                        month = 1;
                        bol = true;
                        break;
                    case "Feb":
                        month = 2;
                        bol = true;
                        break;
                    case "Mar":
                        month = 3;
                        bol = true;
                        break;
                    case "Apr":
                        month = 4;
                        bol = true;
                        break;
                    case "May":
                        month = 5;
                        bol = true;
                        break;
                    case "Jun":
                        month = 6;
                        bol = true;
                        break;
                    case "Jul":
                        month = 7;
                        bol = true;
                        break;
                    case "Aug":
                        month = 8;
                        bol = true;
                        break;
                    case "Sep":
                        month = 9;
                        bol = true;
                        break;
                    case "Oct":
                        month = 10;
                        bol = true;
                        break;
                    case "Nov":
                        month = 11;
                        bol = true;
                        break;
                    case "Dec":
                        month = 12;
                        bol = true;
                        break;

                }
            }

            return bol;
        }

        public void parsStopWord()
        {
            StreamReader reader = new StreamReader(tarPath + @"\stop_words.txt");

            stopDic = new Dictionary<string, bool>();
            stopGigDic = new Dictionary<string, bool>();
            while (reader.Peek() >= 0)
            {
                string line = reader.ReadLine();
                stopDic.Add(line, true);
                if (line.Length > 1)
                    stopGigDic.Add(char.ToUpper(line[0]) + line.Substring(1), true);
                else
                    stopGigDic.Add(char.ToUpper(line[0]) + "", true);

            }
        }
    }
}
