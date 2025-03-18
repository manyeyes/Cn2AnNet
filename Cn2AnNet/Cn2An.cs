using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cn2AnNet
{
    public class Cn2An
    {
        private string allNum;
        private string allUnit;
        private Dictionary<string, string> strictCnNumber;
        private Dictionary<string, string> normalCnNumber;
        private Dictionary<string, string> checkKeyDict;
        private Dictionary<string, Dictionary<string, Regex>> patternDict;
        private An2Cn ac;
        private List<string> modeList;
        private Regex yjfPattern;
        private Regex pattern1;
        private Regex ptnAllNum;
        private Regex ptnSpeakingMode;

        public static Dictionary<string, int> _NUMBER_CN2AN = Conf.NUMBER_CN2AN;
        public static Dictionary<string, int> _UNIT_CN2AN = Conf.UNIT_CN2AN;
        public static Dictionary<string, string> _STRICT_CN_NUMBER = Conf.STRICT_CN_NUMBER;
        public static Dictionary<string, string> _NORMAL_CN_NUMBER = Conf.NORMAL_CN_NUMBER;
        public static Dictionary<int, string> _NUMBER_LOW_AN2CN = Conf.NUMBER_LOW_AN2CN;
        public static Dictionary<int, string> _UNIT_LOW_AN2CN = Conf.UNIT_LOW_AN2CN;

        public Cn2An()
        {
            allNum = string.Join("", _NUMBER_CN2AN.Keys);
            allUnit = string.Join("", _UNIT_CN2AN.Keys);
            strictCnNumber = _STRICT_CN_NUMBER;
            normalCnNumber = _NORMAL_CN_NUMBER;
            checkKeyDict = new Dictionary<string, string>
            {
                {"strict", string.Join("", strictCnNumber.Values) + "点负"},
                {"normal", string.Join("", normalCnNumber.Values) + "点负"},
                {"smart", string.Join("", normalCnNumber.Values) + "点负" + "01234567890.-"}
            };
            patternDict = GetPattern();
            ac = new An2Cn();
            modeList = new List<string> { "strict", "normal", "smart" };
            yjfPattern = new Regex($"^.*?[元圆][{allNum}]角([{allNum}]分)?$");
            pattern1 = new Regex($"^-?\\d+(\\.\\d+)?[{allUnit}]?$");
            ptnAllNum = new Regex($"^[{allNum}]+$");
            ptnSpeakingMode = new Regex($"^([{allNum}]{{0,2}}[{allUnit}])+[{allNum}]$");
        }

        public double Cn2AnConvert(object inputs, string mode = "strict")
        {
            if (inputs == null || inputs.ToString() == "")
            {
                throw new ArgumentException("输入数据为空！");
            }

            if (!modeList.Contains(mode))
            {
                throw new ArgumentException($"mode 仅支持 {string.Join(", ", modeList)} ！");
            }

            string inputStr = inputs.ToString();

            // 数据预处理：
            // 1. 繁体转简体
            // 2. 全角转半角
            inputStr = (string)Proces.Preprocess(inputStr, new List<string>
            {
                "traditional_to_simplified",
                "full_angle_to_half_angle"
            });

            // 特殊转化 廿
            inputStr = inputStr.Replace("廿", "二十");

            // 检查输入数据是否有效
            var (sign, integerData, decimalData, isAllNum) = CheckInputDataIsValid(inputStr, mode);

            // smart 下的特殊情况
            if (sign == 0)
            {
                return double.Parse(integerData);
            }
            else
            {
                double output;
                if (!isAllNum)
                {
                    if (decimalData == null)
                    {
                        output = IntegerConvert(integerData.ToString());
                    }
                    else
                    {
                        output = IntegerConvert(integerData.ToString()) + DecimalConvert(decimalData);
                        // fix 1 + 0.57 = 1.5699999999999998
                        output = Math.Round(output, decimalData.Length);
                    }
                }
                else
                {
                    if (decimalData == null)
                    {
                        output = DirectConvert(integerData.ToString());
                    }
                    else
                    {
                        output = DirectConvert(integerData.ToString()) + DecimalConvert(decimalData);
                        // fix 1 + 0.57 = 1.5699999999999998
                        output = Math.Round(output, decimalData.Length);
                    }
                }
                return sign * output;
            }
        }

        private Dictionary<string, Dictionary<string, Regex>> GetPattern()
        {
            // 整数严格检查
            string _0 = "[零]";
            string _1_9 = "[一二三四五六七八九]";
            string _10_99 = $"{_1_9}?[十]{_1_9}?";
            string _1_99 = $"({_10_99}|{_1_9})";
            string _100_999 = $"({_1_9}[百]([零]{_1_9})?|{_1_9}[百]{_10_99})";
            string _1_999 = $"({_100_999}|{_1_99})";
            string _1000_9999 = $"({_1_9}[千]([零]{_1_99})?|{_1_9}[千]{_100_999})";
            string _1_9999 = $"({_1000_9999}|{_1_999})";
            string _10000_99999999 = $"({_1_9999}[万]([零]{_1_999})?|{_1_9999}[万]{_1000_9999})";
            string _1_99999999 = $"({_10000_99999999}|{_1_9999})";
            string _100000000_9999999999999999 = $"({_1_99999999}[亿]([零]{_1_99999999})?|{_1_99999999}[亿]{_10000_99999999})";
            string _1_9999999999999999 = $"({_100000000_9999999999999999}|{_1_99999999})";
            string strIntPattern = $"^({_0}|{_1_9999999999999999})$";
            string norIntPattern = $"^({_0}|{_1_9999999999999999})$";

            string strDecPattern = "^[零一二三四五六七八九]{0,15}[一二三四五六七八九]$";
            string norDecPattern = "^[零一二三四五六七八九]{0,16}$";

            foreach (var strNum in strictCnNumber.Keys)
            {
                strIntPattern = strIntPattern.Replace(strNum, strictCnNumber[strNum]);
                strDecPattern = strDecPattern.Replace(strNum, strictCnNumber[strNum]);
            }
            foreach (var norNum in normalCnNumber.Keys)
            {
                norIntPattern = norIntPattern.Replace(norNum, normalCnNumber[norNum]);
                norDecPattern = norDecPattern.Replace(norNum, normalCnNumber[norNum]);
            }

            return new Dictionary<string, Dictionary<string, Regex>>
            {
                {
                    "strict", new Dictionary<string, Regex>
                    {
                        {"int", new Regex(strIntPattern)},
                        {"dec", new Regex(strDecPattern)}
                    }
                },
                {
                    "normal", new Dictionary<string, Regex>
                    {
                        {"int", new Regex(norIntPattern)},
                        {"dec", new Regex(norDecPattern)}
                    }
                }
            };
        }

        private string CopyNum(string num)
        {
            string cnNum = "";
            foreach (char n in num)
            {
                cnNum += _NUMBER_LOW_AN2CN[int.Parse(n.ToString())];
            }
            return cnNum;
        }

        private (int sign, string integerData, string decimalData, bool isAllNum) CheckInputDataIsValid(string checkData, string mode)
        {
            // 去除 元整、圆整、元正、圆正
            string[] stopWords = { "元整", "圆整", "元正", "圆正" };
            foreach (string word in stopWords)
            {
                if (checkData.EndsWith(word))
                {
                    checkData = checkData.Substring(0, checkData.Length - 2);
                }
            }

            // 去除 元、圆
            if (mode != "strict")
            {
                string[] normalStopWords = { "圆", "元" };
                foreach (string word in normalStopWords)
                {
                    if (checkData.EndsWith(word))
                    {
                        checkData = checkData.Substring(0, checkData.Length - 1);
                    }
                }
            }

            // 处理元角分
            Match result = yjfPattern.Match(checkData);
            if (result.Success)
            {
                checkData = checkData.Replace("元", "点").Replace("角", "").Replace("分", "");
            }

            // 处理特殊问法：一千零十一 一万零百一十一
            if (checkData.Contains("零十"))
            {
                checkData = checkData.Replace("零十", "零一十");
            }
            if (checkData.Contains("零百"))
            {
                checkData = checkData.Replace("零百", "零一百");
            }

            foreach (char data in checkData)
            {
                if (!checkKeyDict[mode].Contains(data))
                {
                    throw new ArgumentException($"当前为{mode}模式，输入的数据不在转化范围内：{data}！");
                }
            }

            // 确定正负号
            int sign;
            if (checkData.StartsWith("负"))
            {
                checkData = checkData.Substring(1);
                sign = -1;
            }
            else
            {
                sign = 1;
            }

            string integerData;
            string decimalData = null;
            if (checkData.Contains("点"))
            {
                string[] splitData = checkData.Split('点');
                if (splitData.Length == 2)
                {
                    integerData = splitData[0];
                    decimalData = splitData[1];
                    // 将 smart 模式中的阿拉伯数字转化成中文数字
                    if (mode == "smart")
                    {
                        integerData = Regex.Replace(integerData, @"\d+", m => ac.An2CnConvert(int.Parse(m.Value)));
                        decimalData = Regex.Replace(decimalData, @"\d+", m => CopyNum(m.Value));
                        mode = "normal";
                    }
                }
                else
                {
                    throw new ArgumentException("数据中包含不止一个点！");
                }
            }
            else
            {
                integerData = checkData;
                // 将 smart 模式中的阿拉伯数字转化成中文数字
                if (mode == "smart")
                {
                    // 10.1万 10.1
                    Match result1 = pattern1.Match(integerData);
                    if (result1.Success && result1.Value == integerData)
                    {
                        if (_UNIT_CN2AN.ContainsKey(integerData[integerData.Length - 1].ToString()))
                        {
                            double output = double.Parse(integerData.Substring(0, integerData.Length - 1)) * _UNIT_CN2AN[integerData[integerData.Length - 1].ToString()];
                            return (0, output.ToString(), null, false);
                        }
                        else
                        {
                            return (0, integerData, null, false);
                        }
                    }

                    integerData = Regex.Replace(integerData, @"\d+", m => ac.An2CnConvert(int.Parse(m.Value)));
                    mode = "normal";
                }
            }

            Match resultInt = patternDict[mode]["int"].Match(integerData);
            if (resultInt.Success && resultInt.Value == integerData)
            {
                if (decimalData != null)
                {
                    Match resultDec = patternDict[mode]["dec"].Match(decimalData);
                    if (resultDec.Success && resultDec.Value == decimalData)
                    {
                        return (sign, integerData, decimalData, false);
                    }
                }
                else
                {
                    return (sign, integerData, decimalData, false);
                }
            }
            else
            {
                if (mode == "strict")
                {
                    throw new ArgumentException($"不符合格式的数据：{integerData}");
                }
                else if (mode == "normal")
                {
                    // 纯数模式：一二三
                    Match resultAllNum = ptnAllNum.Match(integerData);
                    if (resultAllNum.Success && resultAllNum.Value == integerData)
                    {
                        if (decimalData != null)
                        {
                            Match resultDec = patternDict[mode]["dec"].Match(decimalData);
                            if (resultDec.Success && resultDec.Value == decimalData)
                            {
                                return (sign, DirectConvert(integerData).ToString(), decimalData, true);
                            }
                        }
                        else
                        {
                            return (sign, DirectConvert(integerData).ToString(), decimalData, true);
                        }
                    }

                    // 口语模式：一万二，两千三，三百四，十三万六，一百二十五万三
                    Match resultSpeakingMode = ptnSpeakingMode.Match(integerData);
                    if (integerData.Length >= 3 && resultSpeakingMode.Success && resultSpeakingMode.Value == integerData)
                    {
                        // len(integer_data)>=3: because the minimum length of integer_data that can be matched is 3
                        // to find the last unit
                        char lastUnit = resultSpeakingMode.Groups[1].Value[resultSpeakingMode.Groups[1].Value.Length - 1];
                        string _unit = _UNIT_LOW_AN2CN[_UNIT_CN2AN[lastUnit.ToString()] / 10];
                        integerData = integerData + _unit;
                        if (decimalData != null)
                        {
                            Match resultDec = patternDict[mode]["dec"].Match(decimalData);
                            if (resultDec.Success && resultDec.Value == decimalData)
                            {
                                return (sign, IntegerConvert(integerData).ToString(), decimalData, false);
                            }
                        }
                        else
                        {
                            return (sign, IntegerConvert(integerData).ToString(), decimalData, false);
                        }
                    }
                }
            }

            throw new ArgumentException($"不符合格式的数据：{checkData}");
        }

        private double IntegerConvert(string integerData)
        {
            // 核心
            double outputInteger = 0;
            double unit = 1;
            double tenThousandUnit = 1;
            for (int index = integerData.Length - 1; index >= 0; index--)
            {
                string cnNum = integerData[index].ToString();
                // 数值
                if (_NUMBER_CN2AN.ContainsKey(cnNum))
                {
                    int num = _NUMBER_CN2AN[cnNum];
                    outputInteger += num * unit;
                }
                // 单位
                else if (_UNIT_CN2AN.ContainsKey(cnNum))
                {
                    unit = _UNIT_CN2AN[cnNum];
                    // 判断出万、亿、万亿
                    if (unit % 10000 == 0)
                    {
                        // 万 亿
                        if (unit > tenThousandUnit)
                        {
                            tenThousandUnit = unit;
                        }
                        // 万亿
                        else
                        {
                            tenThousandUnit = unit * tenThousandUnit;
                            unit = tenThousandUnit;
                        }
                    }

                    if (unit < tenThousandUnit)
                    {
                        unit = unit * tenThousandUnit;
                    }

                    if (index == 0)
                    {
                        outputInteger += unit;
                    }
                }
                else
                {
                    break;
                }
            }

            return outputInteger;
        }

        private double DecimalConvert(string decimalData)
        {
            int lenDecimalData = decimalData.Length;

            if (lenDecimalData > 16)
            {
                Console.WriteLine($"注意：小数部分长度为 {lenDecimalData} ，将自动截取前 16 位有效精度！");
                decimalData = decimalData.Substring(0, 16);
                lenDecimalData = 16;
            }

            double outputDecimal = 0;
            for (int index = lenDecimalData - 1; index >= 0; index--)
            {
                int unitKey = _NUMBER_CN2AN[decimalData[index].ToString()];
                outputDecimal += unitKey * Math.Pow(10, -(index + 1));
            }

            // 处理精度溢出问题
            outputDecimal = Math.Round(outputDecimal, lenDecimalData);

            return outputDecimal;
        }

        private double DirectConvert(string data)
        {
            double outputData = 0;
            for (int index = data.Length - 1; index >= 0; index--)
            {
                int unitKey = _NUMBER_CN2AN[data[index].ToString()];
                outputData += unitKey * (int)Math.Pow(10, data.Length - index - 1);
            }

            return outputData;
        }
    }
}