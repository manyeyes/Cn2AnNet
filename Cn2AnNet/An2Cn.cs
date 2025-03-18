using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Cn2AnNet
{
    public class An2Cn
    {
        private readonly string all_num = "0123456789";
        private readonly Dictionary<int, string> number_low=Conf.NUMBER_LOW_AN2CN;
        private readonly Dictionary<int, string> number_up=Conf.NUMBER_UP_AN2CN;
        private readonly List<string> mode_list = new List<string> { "low", "up", "rmb", "direct" };
        private readonly List<string> unit_low_order_an2cn=Conf.UNIT_LOW_ORDER_AN2CN;
        private readonly List<string> unit_up_order_an2cn=Conf.UNIT_UP_ORDER_AN2CN;

        public An2Cn() { }

        public string An2CnConvert(object inputs, string mode = "low")
        {
            if (inputs != null && inputs.ToString() != "")
            {
                if (!mode_list.Contains(mode))
                {
                    throw new ArgumentException($"mode 仅支持 {string.Join(", ", mode_list)} ！");
                }

                string inputStr = inputs is string ? (string)inputs : NumberToString(inputs);

                inputStr = (string)Proces.Preprocess(inputStr);

                CheckInputsIsValid(inputStr);

                string sign = "";
                if (inputStr[0] == '-')
                {
                    sign = "负";
                    inputStr = inputStr.Substring(1);
                }

                string output;
                if (mode == "direct")
                {
                    output = DirectConvert(inputStr);
                }
                else
                {
                    string[] splitResult = inputStr.Split('.');
                    int lenSplitResult = splitResult.Length;
                    if (lenSplitResult == 1)
                    {
                        string integerData = splitResult[0];
                        if (mode == "rmb")
                        {
                            output = IntegerConvert(integerData, "up") + "元整";
                        }
                        else
                        {
                            output = IntegerConvert(integerData, mode);
                        }
                    }
                    else if (lenSplitResult == 2)
                    {
                        string integerData = splitResult[0];
                        string decimalData = splitResult[1];
                        if (mode == "rmb")
                        {
                            string intData = IntegerConvert(integerData, "up");
                            string decData = DecimalConvert(decimalData, "up");
                            int lenDecData = decData.Length;

                            if (lenDecData == 0)
                            {
                                output = intData + "元整";
                            }
                            else if (lenDecData == 1)
                            {
                                throw new ArgumentException($"异常输出：{decData}");
                            }
                            else if (lenDecData == 2)
                            {
                                if (decData[1] != '零')
                                {
                                    if (intData == "零")
                                    {
                                        output = decData[1].ToString() + "角";
                                    }
                                    else
                                    {
                                        output = intData + "元" + decData[1].ToString() + "角";
                                    }
                                }
                                else
                                {
                                    output = intData + "元整";
                                }
                            }
                            else
                            {
                                if (decData[1] != '零')
                                {
                                    if (decData[2] != '零')
                                    {
                                        if (intData == "零")
                                        {
                                            output = decData[1].ToString() + "角" + decData[2].ToString() + "分";
                                        }
                                        else
                                        {
                                            output = intData + "元" + decData[1].ToString() + "角" + decData[2].ToString() + "分";
                                        }
                                    }
                                    else
                                    {
                                        if (intData == "零")
                                        {
                                            output = decData[1].ToString() + "角";
                                        }
                                        else
                                        {
                                            output = intData + "元" + decData[1].ToString() + "角";
                                        }
                                    }
                                }
                                else
                                {
                                    if (decData[2] != '零')
                                    {
                                        if (intData == "零")
                                        {
                                            output = decData[2].ToString() + "分";
                                        }
                                        else
                                        {
                                            output = intData + "元" + "零" + decData[2].ToString() + "分";
                                        }
                                    }
                                    else
                                    {
                                        output = intData + "元整";
                                    }
                                }
                            }
                        }
                        else
                        {
                            output = IntegerConvert(integerData, mode) + DecimalConvert(decimalData, mode);
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"输入格式错误：{inputStr}！");
                    }
                }
                return sign + output;
            }
            else
            {
                throw new ArgumentException("输入数据为空！");
            }
        }

        private string DirectConvert(string inputs)
        {
            string output = "";
            foreach (char d in inputs)
            {
                if (d == '.')
                {
                    output += "点";
                }
                else
                {
                    output += number_low[int.Parse(d.ToString())];
                }
            }
            return output;
        }

        private string NumberToString(object numberData)
        {
            string stringData = numberData.ToString();
            if (stringData.Contains("e"))
            {
                string[] stringDataList = stringData.Split('e');
                string stringKey = stringDataList[0];
                string stringValue = stringDataList[1];
                if (stringValue[0] == '-')
                {
                    stringData = "0." + new string('0', int.Parse(stringValue.Substring(1)) - 1) + stringKey;
                }
                else
                {
                    stringData = stringKey + new string('0', int.Parse(stringValue));
                }
            }
            return stringData;
        }

        private void CheckInputsIsValid(string checkData)
        {
            string allCheckKeys = all_num + ".-";
            foreach (char data in checkData)
            {
                if (!allCheckKeys.Contains(data))
                {
                    throw new ArgumentException($"输入的数据不在转化范围内：{data}！");
                }
            }
        }

        private string IntegerConvert(string integerData, string mode)
        {
            Dictionary<int, string> numeralList;
            List<string> unitList;
            if (mode == "low")
            {
                numeralList = number_low;
                unitList = unit_low_order_an2cn;
            }
            else if (mode == "up")
            {
                numeralList = number_up;
                unitList = unit_up_order_an2cn;
            }
            else
            {
                throw new ArgumentException($"error mode: {mode}");
            }

            integerData = double.Parse(integerData).ToString();

            int lenIntegerData = integerData.Length;
            if (lenIntegerData > unitList.Count)
            {
                throw new ArgumentException($"超出数据范围，最长支持 {unitList.Count} 位");
            }

            string outputAn = "";
            for (int i = 0; i < lenIntegerData; i++)
            {
                int d = int.Parse(integerData[i].ToString());
                if (d != 0)
                {
                    outputAn += numeralList[d] + unitList[lenIntegerData - i - 1];
                }
                else
                {
                    if ((lenIntegerData - i - 1) % 4 == 0)
                    {
                        outputAn += numeralList[d] + unitList[lenIntegerData - i - 1];
                    }

                    if (i > 0 && outputAn[outputAn.Length - 1] != '零')
                    {
                        outputAn += numeralList[d];
                    }
                }
            }

            outputAn = outputAn.Replace("零零", "零").Replace("零万", "万").Replace("零亿", "亿").Replace("亿万", "亿").TrimStart('零').TrimEnd('零');

            if (outputAn.StartsWith("一十"))
            {
                outputAn = outputAn.Substring(1);
            }

            if (string.IsNullOrEmpty(outputAn))
            {
                outputAn = "零";
            }

            return outputAn;
        }

        private string DecimalConvert(string decimalData, string oMode)
        {
            int lenDecimalData = decimalData.Length;

            if (lenDecimalData > 16)
            {
                Console.WriteLine($"注意：小数部分长度为 {lenDecimalData} ，将自动截取前 16 位有效精度！");
                decimalData = decimalData.Substring(0, 16);
            }

            string outputAn = lenDecimalData > 0 ? "点" : "";

            Dictionary<int, string> numeralList;
            if (oMode == "low")
            {
                numeralList = number_low;
            }
            else if (oMode == "up")
            {
                numeralList = number_up;
            }
            else
            {
                throw new ArgumentException($"error mode: {oMode}");
            }

            foreach (char data in decimalData)
            {
                outputAn += numeralList[int.Parse(data.ToString())];
            }
            return outputAn;
        }
    }
}