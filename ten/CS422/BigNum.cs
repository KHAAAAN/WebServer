using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Numerics;


namespace CS422
{
    //spent literally days trying to figure out how to interpret the bits in IEEE754 and convert them to the exact
    //value represented by the double
    //and the answer was right infront of me the whole time :(
    public class BigNum
    {
        private BigInteger _bigInt;

        public BigInteger BigInt
        {
            get
            {
                return _bigInt;
            }
        }

        private int _power;

        public int Power
        {
            get
            {
                return _power;
            }
        }

        private bool _sign = false;
        public bool Sign
        {
            get
            {
                return _sign;
            }
        }

 // (-1)^sign
        private bool _undefined = false;
        private const int _base = 10;

        /* Regex explanation for" @"^\-??[0-9]*\.??[0-9]+$"
         * 
         * ^ = start of string
         * \-?? = 0 or one negative sign
         * [0-9]* = followed by 0 or more characters from 0-0
         * \.?? = followed by 0 or 1 decimal point
         * [0-9]+ = must end with numbers 0-9
         * 
         */

        //number is real number string
        public BigNum(string number)
        {
           
            Initialize(number);

        }

        private void Initialize(string number){

            // first check if number is "" or null
            // then make sure no whitespace allowed
            // then follow above regex pattern which is explained.
            if (string.IsNullOrEmpty(number)
                || Regex.IsMatch(number, @"\s") //\s indicates whitespace
                || !Regex.IsMatch(number, @"^\-??[0-9]*\.??[0-9]*$")
                || Regex.IsMatch(number, @"^\-??$")) //match only one - char
            {
                throw new ArgumentException();
            }

            string bigS = "";
            int curr = 0;

            if (number[curr] == '-')
            {
                _sign = true;
                //bigS += number[curr];
                curr++;

                if (number[curr] == '.')
                {
                    bigS += "0";  
                }
            }
            else if (number[curr] == '.')
            {
                bigS = "0"; 
            }

            while (curr < number.Length && number[curr] != '.')
            {
                bigS += number[curr];
                curr++;
            }

            _power = 0;

            if (curr < number.Length && number[curr] == '.')
            {
                curr++;
            }

            while (curr < number.Length)
            {
                bigS += number[curr];
                _power--; //decimal moving right means power decreases by 1 (base 10)
                curr++;
            }

            _bigInt = BigInteger.Parse(bigS);
        }

        public BigNum(double value, bool useDoubleToString){

            //if double is Nan, +infinity, or -infinity
            if (Double.IsNaN(value) || Double.IsInfinity(value))
            {
                _undefined = true;
            }
            else if (useDoubleToString) //case 2
            {
                Initialize(value.ToString());
            }
            else //value is real number and useDoubleToString is false
            {
                var bytes = BitConverter.GetBytes(value);
                var bits = new BitArray(bytes);

                BigNum product = new BigNum("0");
                bool signBit = bits[bits.Length - 1]; //get index 63.

                BigNum sign = (signBit) ? new BigNum("1") : new BigNum("0");

                //(-1)^sign
                product = BigNum.Pow(new BigNum("-1"), sign);

                //that times (1 + (i=1 to 52 sigma)bit_(52-i) * 2^(-i))
                BigNum sigma = new BigNum("1");

                //fraction bits cover bits b_0 to b_51 starting from b_51
                for (int i = 1; i <= 52; i++)
                {
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

                //b_52 bit to b_62 bit (11 bit biased exponent)
                for (int i = 52; i <= 62; i++, power++)
                {
                    if (bits[i])
                    {
                        //at b_52 power will be 0 and so on.
                        exponent = exponent + BigNum.Pow(new BigNum("2"), power);
                    }
                }

                BigNum n = exponent - new BigNum("1023");

                product = product * BigNum.Pow(new BigNum("2"), new BigNum(n.ToString()));
                Initialize(product.ToString());
            }
        }

        public override string ToString()
        {
            string base10Repr = "";

            if (_undefined)
            {
                return "undefined";
            }
            //number is an integer
            else if (_power == 0)
            {
                base10Repr = _bigInt.ToString();
            }
            else if (_bigInt.IsZero)
            {
                base10Repr = "0";
            }
            else //number is a decimal
            {
                base10Repr = _bigInt.ToString();

                int insertPoint = base10Repr.Length + _power;

                if (insertPoint >= 0)
                {
                    //insert decimal point
                    base10Repr = base10Repr.Insert(insertPoint, ".");
                }
                else //if negative insertPoint, we need to add 0's
                {
                    int i = 0;

                    while (i <= (int)Math.Abs(insertPoint))
                    {
                        base10Repr = "0" + base10Repr;
                        i++;
                    }

                    base10Repr = base10Repr.Insert(1, ".");
                }

                //do not include leading or trailing zeroes.
                base10Repr = base10Repr
                    .TrimStart('0')
                    .TrimEnd('0');
            }

            if (_sign)
            {
                base10Repr = "-" + base10Repr;
            }

            return base10Repr;
        }

        public bool IsUndefined {get{ return _undefined; }}

        public static BigNum operator+(BigNum lhs, BigNum rhs){

            //signs
            //deal with sign 3 cases
            //case 1: x + -y == x - y
            if (!lhs.Sign && rhs.Sign)
            {
                return lhs - (new BigNum("-1") * rhs);
            } //case 2: -x + y == y - x
            else if (lhs.Sign && !rhs.Sign)
            {
                return rhs - (new BigNum("-1") * lhs);
            }

            BigInteger sum;
            int smallerPower = 0;

            if (lhs.Power < rhs.Power)
            {
                smallerPower = lhs.Power;

                //rhs.BigInt * 10^(lhsPower - rhsPower)
                sum = rhs.BigInt * new BigInteger(Math.Pow(_base, Math.Abs(lhs.Power - rhs.Power)));
                sum += lhs.BigInt;
            }
            else if (lhs.Power > rhs.Power)
            {
                smallerPower = rhs.Power;
                sum = lhs.BigInt * new BigInteger(Math.Pow(_base, Math.Abs(lhs.Power - rhs.Power)));
                sum += rhs.BigInt;
            }
            else
            {
                smallerPower = lhs.Power;
                sum = lhs.BigInt + rhs.BigInt;
            }
            
            int insertPoint = sum.ToString().Length + smallerPower;

            string number = sum.ToString();

            number = number.Insert(insertPoint, ".");

            //case 3: -x + -y == -x - y, abs value is equivalent to x + y
            if (lhs.Sign && rhs.Sign)
            {
                number = "-" + number;
            }
            BigNum bigNum = new BigNum(number);

            return bigNum;
        }

        public static BigNum operator-(BigNum lhs, BigNum rhs){

            //3 cases depending on sign for subtraction as well

            //case 1: -x - -y ==  -x + y == y - x
            if (lhs.Sign && rhs.Sign)
            {
                //make into y - x
                return (new BigNum("-1") * rhs) - (new BigNum("-1") * lhs);
            }
            //case 2: x - -y == x + y
            else if (!lhs.Sign && rhs.Sign)
            {
                return lhs + (new BigNum("-1") * rhs);
            }
            //case 3: -x - y == -1(x + y)
            else if (lhs.Sign && !rhs.Sign)
            {
                return (new BigNum("-1")) * ((new BigNum("-1") * lhs) + rhs);
            }

            BigInteger difference;
            int biggerPower = 0, smallerPower = 0;

            if (lhs.Power > rhs.Power)
            {
                smallerPower = rhs.Power;
                biggerPower = lhs.Power;

                //lhs.BigInt * 10^(lhsPower - rhsPower)
                difference = lhs.BigInt * new BigInteger(Math.Pow(_base, Math.Abs(lhs.Power - rhs.Power)));
                difference = difference - rhs.BigInt;
            }
            else if (lhs.Power < rhs.Power)
            {
                smallerPower = lhs.Power;
                biggerPower = rhs.Power;

                //lhs.BigInt * 10^(lhsPower - rhsPower)
                difference = rhs.BigInt * new BigInteger(Math.Pow(_base, Math.Abs(lhs.Power - rhs.Power)));
                difference = lhs.BigInt - difference;
            }
            else
            {
                biggerPower = smallerPower = lhs.Power;
                difference = lhs.BigInt - rhs.BigInt;
            }
              
            int insertPoint = difference.ToString().Length + smallerPower;

            string number = difference.ToString();

            //account for difference of 0
            if (insertPoint >= 0 && insertPoint < number.Length)
            {
                number = number.Insert(insertPoint, ".");
            }
            else if (insertPoint < 0)
            {
                int i = 0;
                bool sign = number.Contains("-");

                if (sign)
                {
                    number = number.TrimStart('-');
                }

                while (i <= (int)Math.Abs(biggerPower))
                {
                    number = "0" + number;
                    i++;
                }

                number = number.Insert(1, ".");
                if (sign)
                {
                    number = "-" + number;
                }
            }

            return new BigNum(number);
        }

        public static BigNum operator*(BigNum lhs, BigNum rhs){

            BigInteger product = lhs.BigInt * rhs.BigInt;
            string number = product.ToString();

            //check how far the powers puts the decimal points.
            int totalPower = Math.Abs(lhs.Power) + Math.Abs(rhs.Power);

            int insertPoint = number.Length - totalPower;

            if (insertPoint >= 0 && insertPoint < number.Length)
            {
                number = number.Insert(insertPoint, ".");
            }
            else if (insertPoint < 0)
            {
                int i = 0;

                while (i >= insertPoint)
                {
                    number = "0" + number;
                    i--;
                }

                number = number.Insert(1, ".");
            }

            //- * + || + * -, - * - cancels out.
            if (lhs.Sign && !rhs.Sign || !lhs.Sign && rhs.Sign)
            {
                number = "-" + number;
            }

            return new BigNum(number);
        }

        public static BigNum operator++(BigNum bigNum){
            return bigNum + new BigNum("1");
        }

        public static BigNum Pow(BigNum baseNum, BigNum power){
            BigNum number = new BigNum("1");

            if (baseNum.Power != 0)
            {
                throw new Exception("baseNum must be whole BigNum i.e Power has to be 0");
            } 

            string baseNumString = baseNum.ToString();

            for(BigNum i = new BigNum("0"); i.BigInt < power.BigInt; i++){
                number = number * new BigNum(baseNumString);
            }

            if (power.Sign)
            {
                number = new BigNum("1") / number;
            }

            return number;
        }

        public static BigNum operator / (BigNum lhs, BigNum rhs){
            //account for a hundred digits.
            int threshold = 100;

            BigInteger dividend = lhs.BigInt * BigInteger.Pow(_base, threshold);
            BigInteger bigIntQuotient = dividend / rhs.BigInt;
            BigNum bigNumQuotient = new BigNum(bigIntQuotient.ToString());

            int newPower = bigNumQuotient.Power - threshold;
            int lhsPower = Math.Abs(lhs.Power);
            int rhsPower = Math.Abs(rhs.Power);

            newPower -= lhsPower; //dividend causes . to move <
            newPower += rhsPower; //divisor causes . to move >

            bigNumQuotient._power = newPower;
            if (lhs.Sign && !rhs.Sign || !rhs.Sign && lhs.Sign)
            {
                bigNumQuotient._sign = true;
            }
            //get rid of any decimal points at the end.
            bigNumQuotient = new BigNum(bigNumQuotient.ToString());

            return bigNumQuotient;
        }


        private static bool lessThan(BigNum lhs, BigNum rhs, bool equalTo = false){
            BigNum difference = lhs - rhs;
            bool less = false;

            if (difference.BigInt == 0) //special zero case
            {
                if (equalTo)
                {
                    less = true;
                }
            } 
            else if (!lhs.Sign && !rhs.Sign)
            {
                if (difference.Sign)
                {
                    less = true;
                }
            }
            else if (lhs.Sign && rhs.Sign)
            {
                if (!difference.Sign)
                {
                    less = true;
                }
            }
            else if (lhs.Sign && !rhs.Sign)
            {
                less = true;
            }

            return less;
        }

        public static bool operator < (BigNum lhs, BigNum rhs){
           
            return lessThan(lhs, rhs);

        }

        public static bool operator <= (BigNum lhs, BigNum rhs){

            return lessThan(lhs, rhs, true);
        }

        private static bool greaterThan (BigNum lhs, BigNum rhs, bool equalTo = false){
            BigNum difference = lhs - rhs;
            bool greater = false;

            if (difference.BigInt == 0) //special zero case
            {
                if (equalTo)
                {
                    greater = true;
                }
            } 
            else if (!lhs.Sign && !rhs.Sign)
            {
                if (!difference.Sign)
                {
                    greater = true;
                }
            }
            else if (lhs.Sign && rhs.Sign)
            {
                if (difference.Sign)
                {
                    greater = true;
                }
            }
            else if (!lhs.Sign && rhs.Sign)
            {
                greater = true;
            }

            return greater;
        }

        public static bool operator > (BigNum lhs, BigNum rhs){

            return greaterThan(lhs, rhs);
        }

        public static bool operator >= (BigNum lhs, BigNum rhs){

            return greaterThan(lhs, rhs, true);
        }

        public static bool IsToStringCorrect(double value){
            BigNum bigNum = new BigNum(value, false);
            string checkToString = value.ToString();

            bool success = false;

            if (bigNum.ToString() == checkToString)
            {
                success = true;
            }

            return success;
        }
    }
}

