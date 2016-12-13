using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Numerics;
using CS422;

namespace HW10
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            /*string whitespaceTest = "1.2\n34\n";
            Regex whitespace = new Regex(@"\s");

            Console.WriteLine(whitespace.IsMatch(whitespaceTest));

            string startSignTest1 = "11";
            string startSignTest2 = "1.234";
            string startSignTest3 = ".1234";
            string startSignTest4 = "-1.234";
            string startSignTest5 = "-.1234";
            string startSignTest6 = "-123.34";*/

            //Regex startSignRegex = new Regex(@"^[0-9[\.??[0-9]]\-[0-9\.[0-9]]\.[0-9]].$");
            /*** Regex explanation:
             * 
             * ^ = start of string
             * \-?? = 0 or one negative sign
             * [0-9]* = followed by 0 or more characters from 0-0
             * \.?? = followed by 0 or 1 decimal point
             * [0-9]+ = must end with numbers 0-9
             */
            /*Regex startSignRegex = new Regex(@"^\-??[0-9]*\.??[0-9]+$");

            Console.WriteLine(startSignRegex.IsMatch(startSignTest1));
            Console.WriteLine(startSignRegex.IsMatch(startSignTest2));
            Console.WriteLine(startSignRegex.IsMatch(startSignTest3));
            Console.WriteLine(startSignRegex.IsMatch(startSignTest4));
            Console.WriteLine(startSignRegex.IsMatch(startSignTest5));
            Console.WriteLine(startSignRegex.IsMatch(startSignTest6));

            Regex testRegex = new Regex(@"^[0-9][0-9]$");
            Console.WriteLine(testRegex.IsMatch(startSignTest1));*/

            //string number = "";
            //double value = 0.0009765625;
            //double value = 126643226436436432;
            /*double value = 9.4;

            //double value = 1797693134862332911;

            Console.WriteLine("value = {0}", value.ToString("R"));

            var bytes = BitConverter.GetBytes(value);
            var bits = new BitArray(bytes);

            double product = 0;
            double signBit = Convert.ToDouble(bits[bits.Length - 1]); //get index 63.
            Console.WriteLine("signBit = {0}", signBit);
            //(-1)^sign
            product = Math.Pow(-1, signBit);


            //that times (1 + (i=1 to 52 sigma)bit_(52-i) * 2^(-i))
           
            double sigma = 1;
            double fraction = 0;
            double baseNum = 0;
            //fraction bits cover bits b_0 to b_51 starting from b_51
            for (int i = 1; i <= 52; i++)
            {
                //bit_(52-i) where bit_0 (52-52) is first bit.
                double bit = Convert.ToDouble(bits[52 - i]);
                Console.WriteLine(bit);

                sigma += bit * Math.Pow(2, -i);
            }

            Console.WriteLine("decimal value of significand = {0}, fraction = {1}", sigma.ToString("r"),
                fraction.ToString());

            //(-1)^sign * (1 + (i=1 to 52 sigma)bit_(52-i) * 2^(-i)) 
            //at this point
            product *= sigma;

            //product *= Math.Pow(2, 1023 - 1023);
            //Console.WriteLine("product = {0}", product);

            //now have to do 2^(e-1023) where e is
            //our biased exponent (the 11 bit value) from the end

            double exponent = 0;
            double power = 0;

            Console.WriteLine("\n\nbits: ");
            //b_52 bit to b_62 bit (11 bit biased exponent)
            for (int i = 52; i <= 62; i++, power++)
            {
                if (bits[i])
                {
                    //at b_52 power will be 0 and so on.
                    exponent += Math.Pow(2, power);
                }

                Console.WriteLine(Convert.ToByte(bits[i]));
            }

            double n = (exponent - 1023);

            if (value >= 1)
            {
                baseNum += Math.Pow(2, n);
                int j = 51;

                for (double i = n - 1; i >= 0 && j >= 0; i--, j--)
                {
                    if (bits[j])
                    {
                        baseNum += Math.Pow(2, i);
                    }
                }
            }


            double dec = 0;
            double fullNum = 0;
            int count = 0;
            BigNum bigDecimal = new BigNum("0");
            BigNum one = new BigNum("1");

            if (value >= 1)
            {
                Console.WriteLine("\n\ndec:");
                for (int i = 1 + (int)n; i <= 52; i++)
                {
                    double bit = Convert.ToDouble(bits[52 - i]);
                    Console.WriteLine("bit * Math.Pow({1}, {2}) = {0}", bit * Math.Pow(2, -(i - (int)n)), 2, -(i - (int)n));
                    dec += bit * Math.Pow(2, -(i - (int)n));
                    Console.WriteLine("dec = {0}", dec);
                    Console.WriteLine("bit = {0}", bit);
                    count++;

                    BigNum twoMultiplier = new BigNum("1");

                    //i.e get 2^i where i =49, need to start at 0 to ensure we multiply with our base of 2
                    for (int j = 1; j <= (i - (int)n); j++) 
                    {
                        twoMultiplier = twoMultiplier * new BigNum("2");
                    }

                    //then do 1/(2^49)
                    if (bit == 1)
                    {
                        BigNum bigBit = one / twoMultiplier;
                        bigDecimal = bigDecimal + bigBit;
                        Console.WriteLine("bigDecimal = {0}, bigBit = {1}\n", bigDecimal, bigBit);
                    }
                    else
                    {
                        Console.WriteLine("bigDecimal = {0}, bigBit = {1}\n", bigDecimal, 0);
                    }
                }
            }
            else
            {
                Console.WriteLine("\n\ndec:");
                for (int i = 1 + (int)n; i <= 52; i++)
                {
                    double bit = Convert.ToDouble(bits[52 - i]);
                    Console.WriteLine("bit * Math.Pow({1}, {2}) = {0}", bit * Math.Pow(2, -(i - (int)n)), 2, -
                        (i - (int)n));

                    dec += bit * Math.Pow(2, -(i - (int)n));
                    Console.WriteLine("dec = {0}", dec);
                    Console.WriteLine("bit = {0}\n", bit);
                    count++;
                }
            }

            count -= 1;
            fullNum = baseNum + dec;

            Console.WriteLine("baseNum = {0}, dec = {1}, count = {2}, add = {3}, equals = {4}, fullNum = {5}",
                baseNum, dec, count, baseNum + dec,
                (baseNum+dec).Equals(9.4), fullNum.ToString("R"));
            
         
            //1023 is our biased exponent
            Console.WriteLine("Exponent = {0}, 2^(e - 1023) = {1}", exponent, Math.Pow(2, exponent - 1023) );


            product *= Math.Pow(2, exponent - 1023);

            Console.WriteLine("product = {0}", product.ToString("R"));
            //var bi = BigInteger.Parse("12345678910111234567891012345");
            //Console.WriteLine(bi.ToString("R"));

            /*BigNum x = new BigNum("1");
            BigNum y = new BigNum("1");
            for (int i = 1; i <= 49; i++)
            {
                x = x* new BigNum("2");
            }

            Console.WriteLine("x = {0}, y = {1}, y/x = {2}", x, y, (y/x));

            BigNum bigNum = new BigNum("4503599627370496");
            BigNum bigNum2 = new BigNum("1");
            BigNum res = bigNum2 / bigNum;

            Console.WriteLine(res);

            //double test = 126643226436436432;
            //Console.WriteLine(test);

            */

            double value = 0.0009765625123456789123456;
            //double value = 126643226436436432;
            //double value = 9.4;

            //double value = 1797693134862332911;

            Console.WriteLine("value = {0}", value.ToString("R"));

            var bytes = BitConverter.GetBytes(value);
            var bits = new BitArray(bytes);

            BigNum product = new BigNum("0");
            bool signBit = bits[bits.Length - 1]; //get index 63.

            Console.WriteLine("signBit = {0}", signBit);
            BigNum sign = (signBit) ? new BigNum("1") : new BigNum("0");

            //(-1)^sign
            product = BigNum.Pow(new BigNum("-1"), sign);
            Console.WriteLine("product = {0}", product);

            //that times (1 + (i=1 to 52 sigma)bit_(52-i) * 2^(-i))
            BigNum sigma = new BigNum("1");

            //fraction bits cover bits b_0 to b_51 starting from b_51
            for (int i = 1; i <= 52; i++)
            {
                //bit_(52-i) where bit_0 (52-52) is first bit.
                double bit = Convert.ToDouble(bits[52 - i]);

                Console.WriteLine(bit);

                if (bits[52 - i])
                {
                    sigma += BigNum.Pow(new BigNum("2"),
                        new BigNum((-i).ToString()));
                }
            }

            product = product * sigma;

            //now have to do 2^(e-1023) where e is
            //our biased exponent (the 11 bit value) from the end

            BigNum exponent = new BigNum("0");
            BigNum power = new BigNum("0");

            Console.WriteLine("\n\nbits: ");
            //b_52 bit to b_62 bit (11 bit biased exponent)
            for (int i = 52; i <= 62; i++, power++)
            {
                if (bits[i])
                {
                    //at b_52 power will be 0 and so on.
                    exponent = exponent + BigNum.Pow(new BigNum("2"), power);
                }

                Console.WriteLine(Convert.ToByte(bits[i]));
            }

            BigNum n = exponent - new BigNum("1023");
            Console.WriteLine("n = {0}", n);

            Console.WriteLine("product = {0}, product.Power = {1}", product.BigInt, product.Power);

            product = product * BigNum.Pow(new BigNum("2"), new BigNum(n.ToString()));

            Console.WriteLine("product = {0}, product.Power = {1}", product.BigInt, product.Power);

            BigNum test = new BigNum("9");
            BigNum test2 = new BigNum("9");
            double test3 = 1234568901234567;
            Console.WriteLine(test3);
            Console.WriteLine(BigNum.IsToStringCorrect(test3));

            BigInteger bi1 = BigInteger.Parse("1000012512");
            BigInteger bi2 = BigInteger.Parse("200013");

            Console.WriteLine(bi1 / bi2);


        }

    }
}
