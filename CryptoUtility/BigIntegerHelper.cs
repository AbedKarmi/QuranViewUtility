﻿using Org.BouncyCastle.Crypto.Prng;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

internal static class BigIntegerHelper
{
    public enum NumberFormat
    {
        Binary = 0,
        Decimal,
        Hexadecimal,
        Base64
    }

    private static readonly BigInteger Ten = new(10);

    public static BigInteger ToBigInteger(this ulong ul)
    {
        return new BigInteger(ul);
    }

    public static BigInteger ToBigInteger(this long ul)
    {
        return new BigInteger((ulong)ul);
    }

    public static BigInteger ToBigInteger(this int ul)
    {
        return new BigInteger((ulong)ul);
    }

    public static BigInteger ToBigInteger(this uint ul)
    {
        return new BigInteger((ulong)ul);
    }

    public static BigInteger PowBySquaring(BigInteger x, BigInteger n)
    {
        if (n < 0) return PowBySquaring(BigInteger.Divide(1, x), -n);
        if (n == 0) return 1;
        if (n == 1) return x;
        if (n.IsEven) return PowBySquaring(BigInteger.Multiply(x, x), BigInteger.Divide(n, 2));

        return BigInteger.Multiply(x,
            PowBySquaring(BigInteger.Multiply(x, x), BigInteger.Divide(BigInteger.Subtract(n, 1), 2)));
    }

    public static BigInteger FastPow(BigInteger x, BigInteger n)
    {
        BigInteger a = 1;
        var c = x;

        do
        {
            var r = n % 2;
            if (r == 1) a = KaratsubaMultiply(1, c);
            n >>= 1;
            c = KaratsubaMultiply(c, c);
        } while (n != 0);

        return a;
    }

    public static BigInteger GetBig(string hexNum, bool forcePositive = true)
    {
        //return GetBig(MyClass.HexStringToBinary(hexNum));
        if (hexNum.StartsWith("0x")) hexNum = hexNum.Substring(2);
        return GetBig(HexToByteArray(hexNum), forcePositive);
    }

    private static byte[] Reverse(byte[] arr)
    {
        var rvArr=new byte[arr.Length];
        Array.Copy(arr, 0, rvArr, 0, arr.Length);
        Array.Reverse(rvArr);
        return rvArr;
    }
    public static BigInteger GetBig(byte[] data, bool forcePositive = true)
    {
       if (data.Length == 0)
            return new BigInteger(data);

        var n = data.Length;
        //while (data[n - 1] == 0 && n > 1) n--; // Remove Left Zero Before Reverse, otherwise will be to the right !

        var inArr = new byte[n];
        Array.Copy(data, inArr, n);
        //byte[] inArr = (byte[])data.Clone();
        var m = (forcePositive && inArr[0]!=0 ? 1 : 0); // Force positive
        Array.Reverse(inArr); // Reverse the byte order
        var final = new byte[inArr.Length + m]; // Add an empty byte at the end, to simulate unsigned BigInteger (no negatives!)
        Array.Copy(inArr, final, inArr.Length);

        return new BigInteger(final);
   
    }

    public static BigInteger KaratsubaMultiply(BigInteger x, BigInteger y)
    {
        var size1 = GetSize(x);
        var size2 = GetSize(y);

        //find the max size of two integers
        var N = Math.Max(size1, size2);

        if (N < 2)
            return x * y;

        //Max length divided by two and rounded up
        N = N / 2 + N % 2;

        //The mulitplying factor for calculating a,b,c,d
        var m = BigInteger.Pow(10, N);

        var b = x % m;
        var a = x / m;
        var c = y / m;
        var d = y % m;

        var z0 = KaratsubaMultiply(a, c);
        var z1 = KaratsubaMultiply(b, d);
        var z2 = KaratsubaMultiply(a + b, c + d);

        return BigInteger.Pow(10, N * 2) * z0 + z1 + (z2 - z1 - z0) * m;
    }

    /// <summary>
    ///     returns the size of the long integers
    /// </summary>
    /// <param name="num"></param>
    /// <returns></returns>
    private static int GetSize(BigInteger num)
    {
        var len = 0;
        while (num != 0)
        {
            len++;
            num /= 10;
        }

        return len;
    }

    public static bool IsPrime(BigInteger n)
    {
        if (n <= 1) return false;
        if (n <= 3) return true;
        if (n % 2 == 0 || n % 3 == 0) return false;
        BigInteger i = 5;
        while (BigInteger.Multiply(i, i) <= n)
        {
            if (n % i == 0 || n % (i + 2) == 0) return false;
            i += 6;
        }

        return true;
    }

    public static BigInteger Sqrt(this BigInteger number)
    {
        BigInteger n = 0, p = 0;
        if (number == BigInteger.Zero)
            return BigInteger.Zero;
        var high = number >> 1;
        var low = BigInteger.Zero;
        while (high > low + 1)
        {
            n = (high + low) >> 1;
            p = n * n;
            if (number < p)
                high = n;
            else if (number > p)
                low = n;
            else
                break;
        }

        return number == p ? n : low;
    }

    /// <summary>
    ///     Creates a new BigInteger from a binary (Base2) string
    /// </summary>
    public static BigInteger NewBigInteger2(this string binaryValue)
    {
        BigInteger res = 0;
        if (binaryValue.Count(b => b == '1') + binaryValue.Count(b => b == '0') != binaryValue.Length) return res;
        foreach (var c in binaryValue)
        {
            res <<= 1;
            res += c == '1' ? 1 : 0;
        }

        return res;
    }

    /// <summary>
    ///     Get the bitwidth of this biginteger n
    /// </summary>
    public static int GetActualBitwidth(this BigInteger n)
    {
        var i = 0;
        while (n > 0)
        {
            n >>= 1;
            i++;
        }

        return i;
    }

    public static int AdjustBits(int n)
    {
        var m = n % 8;
        if (m > 0) n += 8 - m;
        return n;
    }

    public static int GetBitwidth(this BigInteger n)
    {
        var b = n.ToByteArray();
        var i = b.Length;
        if (b[b.Length - 1] == 0) i--;
        return i << 3;
    }

    public static bool IsEven(this int num)
    {
        return (num & 1) == 0;
    }

    public static bool ContainsOnly(this string str, string digits)
    {
        foreach (var c in str)
            if (!digits.Contains(c))
                return false;
        return true;
    }

    /// <summary>
    ///     Get the Maxvalue for a biginteger of this bitlength
    /// </summary>
    public static BigInteger GetMaxValue(int bitlength)
    {
        var buffer = "";
        if (!bitlength.IsEven())
            buffer = "7f";
        var ByteLength = bitlength >> 3;
        for (var i = 0; i < ByteLength; ++i)
            buffer += "ff";
        return ToBigInteger16(buffer);
    }

    /// <summary>
    ///     Converts a hex number (0xABCDEF or ABCDEF) into a BigInteger
    /// </summary>
    public static BigInteger ToBigInteger16(this string hexNumber)
    {
        if (string.IsNullOrEmpty(hexNumber))
            throw new Exception("Error: hexNumber cannot be either null or have a length of zero.");
        if (!hexNumber.ContainsOnly("0123456789abcdefABCDEFxX"))
            throw new Exception("Error: hexNumber cannot contain characters other than 0-9,a-f,A-F, or xX");
        hexNumber = hexNumber.ToUpper();
        if (hexNumber.IndexOf("0X", StringComparison.OrdinalIgnoreCase) != -1)
            hexNumber = hexNumber.Substring(2);
        var bytes = Enumerable.Range(0, hexNumber.Length)
            .Where(x => x % 2 == 0)
            .Select(x => Convert.ToByte(hexNumber.Substring(x, 2), 16))
            .ToArray();
        return new BigInteger(bytes.Concat(new byte[] { 0 }).ToArray());
    }

    /// <summary>
    ///     Creates a new BigInteger from a BigInteger
    /// </summary>
    public static BigInteger NewBigInteger(this BigInteger value)
    {
        return new BigInteger(value.ToByteArray());
    }

    /// <summary>
    ///     Creates a new BigInteger from a hex (Base16) string
    /// </summary>
    public static BigInteger NewBigInteger16(this string hexValue)
    {
        return new BigInteger(HexToByteArray(hexValue).Concat(new byte[] { 0 }).ToArray());
    }

    /// <summary>
    ///     Creates a new BigInteger from a number (Base10) string
    /// </summary>
    public static BigInteger NewBigInteger10(this string str)
    {
        if (str[0] == '-')
            throw new Exception("Invalid numeric string. Only positive numbers are allowed.");
        var number = new BigInteger();
        int i;
        for (i = 0; i < str.Length; i++)
            if (str[i] >= '0' && str[i] <= '9')
                number = number * Ten + long.Parse(str[i].ToString());
        return number;
    }

    public static BigInteger ToBigIntegerBase10(this string str)
    {
        if (str[0] == '-')
            throw new Exception("Invalid numeric string. Only positive numbers are allowed.");
        var number = new BigInteger();
        int i;
        for (i = 0; i < str.Length; i++)
            if (str[i] >= '0' && str[i] <= '9')
                number = number * Ten + long.Parse(str[i].ToString());
        return number;
    }

    /// <summary>
    ///     Return a byte array that represents this hex string
    /// </summary>
    private static byte[] HexToByteArray(string hex)
    {
        byte[] hr;
        try
        {
            if (!hex.Length.IsEven()) hex = "0" + hex;

            hr = Enumerable.Range(0, hex.Length)
                .Where(x => x % 2 == 0)
                .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                .ToArray();
        }
        catch (Exception)
        {
            hr = new byte[0] { };
        }

        ;
        return hr;
    }

    /// <summary>
    ///     Ensures that the BigInteger value will be a positive number. BigInteger is Big-endian
    ///     (the most significant byte is in the [0] position)
    /// </summary>
    public static byte[] EnsurePositiveNumber(this byte[] ba)
    {
        return ba.Concat(new byte[] { 0 }).ToArray();
    }

    /// <summary>
    ///     Converts from a BigInteger to a binary string.
    /// </summary>
    public static string ToBinaryString(this BigInteger bigint)
    {
        var bytes = bigint.ToByteArray();
        Array.Reverse(bytes);
        var base2 = new StringBuilder(bytes.Length * 8);
        var binary = Convert.ToString(bytes[0], 2);
        if (binary[0] != '0' && bigint.Sign == 1) base2.Append('0');
        base2.Append(binary);
        for (var index = 1; index < bytes.Length; index++)
            base2.Append(Convert.ToString(bytes[index], 2).PadLeft(8, '0'));
        return base2.ToString();
    }

    /// <summary>
    ///     Converts from a BigInteger to a hexadecimal string.
    /// </summary>
    public static string ToHexString(this BigInteger bi)
    {
        var bytes = bi.ToByteArray();
        Array.Reverse(bytes);
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            var hex = b.ToString("X2");
            sb.Append(hex);
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Converts from a BigInteger to a octal string.
    /// </summary>
    public static string ToOctalString(this BigInteger bigint)
    {
        var bytes = bigint.ToByteArray();
        Array.Reverse(bytes);
        var index = bytes.Length - 1;
        var base8 = new StringBuilder((bytes.Length / 3 + 1) * 8);
        var rem = bytes.Length % 3;
        if (rem == 0) rem = 3;
        var base0 = 0;
        while (rem != 0)
        {
            base0 <<= 8;
            base0 += bytes[index--];
            rem--;
        }

        var octal = Convert.ToString(base0, 8);
        if (octal[0] != '0' && bigint.Sign == 1) base8.Append('0');
        base8.Append(octal);
        while (index >= 0)
        {
            base0 = (bytes[index] << 16) + (bytes[index - 1] << 8) + bytes[index - 2];
            base8.Append(Convert.ToString(base0, 8).PadLeft(8, '0'));
            index -= 3;
        }

        return base8.ToString();
    }

    /*
* public void testPrimes()
      {

                  int bitlen = 2048;
                  BigInteger RandomNumber = NextBigInteger(bitlen);

                  if (IsProbablePrime(RandomNumber, 100) == true)
                      Console.WriteLine("\nGenerated Random number is prime!");
                  else
                      Console.WriteLine("\nGenerated Random number is not prime!!");
      }
*/

    public static IList<BigInteger> GetFactors(BigInteger n)
    {
        List<BigInteger> factors = new();
        BigInteger x = 2;
        while (x <= n)
            if (n % x == 0)
            {
                factors.Add(x);
                n /= x;
            }
            else
            {
                x++;
                if (x * x >= n)
                {
                    factors.Add(n);
                    break;
                }
            }

        return factors;
    }

public struct Factors { public BigInteger Factor { get; set; } public int Count { get; set; } }
    
// function to calculate all the prime factors and
// count of every prime factor
public static List<Factors> Factorize(BigInteger n)
{
    int count = 0;
    BigInteger two = 2;
    List<Factors> factors = new();

    // count the number of times 2 divides
    while (n.IsEven)
    {
        n >>= 1; // equivalent to n=n/2;Faster to shift rather than divide
        count++;
    }
    // if 2 divides it
    if (count != 0) factors.Add(new Factors {Factor=2, Count=count });


    // check for all the possible numbers that can divide it
    for (BigInteger i = 3; i <= n.Sqrt(); i += 2)
     // foreach (var i in GetPrimes_SieveOfAtkin(n.Sqrt()))
     {
        count = 0;
        while ((n % i) == 0)
        {
            count++;
            n /= i;
        }
        if (count!=0)  factors.Add(new Factors { Factor = i, Count = count });
     }

     // if n at the end is a prime number.
     if (n > 2) factors.Add(new Factors { Factor = n, Count = 1 }); 

     return factors;
}

 public static SortedSet<BigInteger> GetPrimes_SieveOfAtkin(BigInteger limit)
    {
        // 2 and 3 are known to be prime
        if (limit > 2)
            Console.Write(2 + " ");

        if (limit > 3)
            Console.Write(3 + " ");

        // Initialise the sieve array with
        // false values
        SortedSet<BigInteger> sieve = new();

       /* Mark sieve[n] is true if one of the
        following is true:
        a) n = (4*x*x)+(y*y) has odd number
           of solutions, i.e., there exist
           odd number of distinct pairs
           (x, y) that satisfy the equation
           and    n % 12 = 1 or n % 12 = 5.
        b) n = (3*x*x)+(y*y) has odd number
           of solutions and n % 12 = 7
        c) n = (3*x*x)-(y*y) has odd number
           of solutions, x > y and n % 12 = 11 */
        for (int x = 1; x * x <= limit; x++)
        {
            for (int y = 1; y * y <= limit; y++)
            {

                // Main part of Sieve of Atkin
                int n = (4 * x * x) + (y * y);
                if (n <= limit
                    && (n % 12 == 1 || n % 12 == 5))

                     sieve.Add(n);

                n = (3 * x * x) + (y * y);
                if (n <= limit && n % 12 == 7)
                    sieve.Add(n);

                n = (3 * x * x) - (y * y);
                if (x > y && n <= limit
                    && n % 12 == 11)
                    sieve.Add(n);
            }
        }

        // Mark all multiples of squares as
        // non-prime
        for (int r = 5; r * r < limit; r++)
        {
            if (sieve.Contains(r))
            {
                for (int i = r * r; i < limit;
                     i += r * r)
                    sieve.Remove(i);
            }
        }

        return sieve;
    }
    public static BigInteger[] GcdWithBezout(BigInteger p, BigInteger q)
    {
        if (q == 0)
            return new[] { p, 1, 0 };

        var vals = GcdWithBezout(q, p % q);
        var d = vals[0];
        var a = vals[2];
        var b = vals[1] - p / q * vals[2];

        return new[] { d, a, b };
    }

    public static BigInteger GenPrime(int bitLength)
    {
        BigInteger p;
        do
        {
            p = NextBigInteger(bitLength);
        } while (!IsProbablePrime(p, 100));

        return p;
    }

    public static BigInteger NextBigInteger(int bitLength)
    {
        if (bitLength < 1) return BigInteger.Zero;

        var bytes = bitLength / 8;
        var bits = bitLength % 8;

        // Generates enough random bytes to cover our bits.
        Random rnd = new();
        var bs = new byte[bytes + 1];
        rnd.NextBytes(bs);

        // Mask out the unnecessary bits.
        var mask = (byte)(0xFF >> (8 - bits));
        bs[bs.Length - 1] &= mask;

        return new BigInteger(bs);
    }

    // Random Integer Generator within the given range
    public static BigInteger RandomBigInteger(BigInteger start, BigInteger end)
    {
        if (start == end) return start;

        var res = end;

        // Swap start and end if given in reverse order.
        if (start > end)
        {
            end = start;
            start = res;
            res = end - start;
        }
        else
            // The distance between start and end to generate a random BigIntger between 0 and (end-start) (non-inclusive).
        {
            res -= start;
        }

        var bs = res.ToByteArray();

        // Count the number of bits necessary for res.
        var bits = 8;
        byte mask = 0x7F;
        while ((bs[bs.Length - 1] & mask) == bs[bs.Length - 1])
        {
            bits--;
            mask >>= 1;
        }

        bits += 8 * bs.Length;

        // Generate a random BigInteger that is the first power of 2 larger than res, 
        // then scale the range down to the size of res,
        // finally add start back on to shift back to the desired range and return.
        return NextBigInteger(bits + 1) * res / BigInteger.Pow(2, bits + 1) + start;
    }


    // Miller-Rabin primality test as an extension method on the BigInteger type.
    // Based on the Ruby implementation on this page.

    public static bool IsProbablePrime(BigInteger source, int certainty)
    {
        if (source == 2 || source == 3)
            return true;
        if (source < 2 || source % 2 == 0)
            return false;

        var d = source - 1;
        var s = 0;

        while (d % 2 == 0)
        {
            d /= 2;
            s += 1;
        }

        // There is no built-in method for generating random BigInteger values.
        // Instead, random BigIntegers are constructed from randomly generated
        // byte arrays of the same length as the source.
        var rng = RandomNumberGenerator.Create();
        var bytes = new byte[source.ToByteArray().LongLength];
        BigInteger a;

        for (var i = 0; i < certainty; i++)
        {
            do
            {
                // This may raise an exception in Mono 2.10.8 and earlier.
                // http://bugzilla.xamarin.com/show_bug.cgi?id=2761
                rng.GetBytes(bytes);
                a = new BigInteger(bytes);
            } while (a < 2 || a >= source - 2);

            var x = BigInteger.ModPow(a, d, source);
            if (x == 1 || x == source - 1)
                continue;

            for (var r = 1; r < s; r++)
            {
                x = BigInteger.ModPow(x, 2, source);
                if (x == 1)
                    return false;
                if (x == source - 1)
                    break;
            }

            if (x != source - 1)
                return false;
        }

        return true;
    }

    public static BigInteger MaxValue { get { return BigInteger.Pow(2, 256); } }

    public static BigInteger Modinv(this BigInteger u, BigInteger v)
    {
        BigInteger inv, u1, u3, v1, v3, t1, t3, q;
        BigInteger iter;
        /* Step X1. Initialise */
        u1 = 1;
        u3 = u;
        v1 = 0;
        v3 = v;
        /* Remember odd/even iterations */
        iter = 1;
        /* Step X2. Loop while v3 != 0 */
        while (v3 != 0)
        {
            /* Step X3. Divide and "Subtract" */
            q = u3 / v3;
            t3 = u3 % v3;
            t1 = u1 + q * v1;
            /* Swap */
            u1 = v1;
            v1 = t1;
            u3 = v3;
            v3 = t3;
            iter = -iter;
        }

        /* Make sure u3 = gcd(u,v) == 1 */
        if (u3 != 1)
            return 0; /* Error: No inverse exists */
        /* Ensure a positive result */
        if (iter < 0)
            inv = v - u1;
        else
            inv = u1;
        return inv;
    }


    /// <summary>
    /// Calculates the modular multiplicative inverse of <paramref name="a"/> modulo <paramref name="m"/>
    /// using the extended Euclidean algorithm.
    /// </summary>
    /// <remarks>
    /// This implementation comes from the pseudocode defining the inverse(a, n) function at
    /// https://en.wikipedia.org/wiki/Extended_Euclidean_algorithm
    /// </remarks>
    public static BigInteger ModInverse(this BigInteger a, BigInteger n)
    {
        BigInteger t = 0, nt = 1, r = n, nr = a;

        if (n < 0)
        {
            n = -n;
        }

        if (a < 0)
        {
            a = n - (-a % n);
        }

        while (nr != 0)
        {
            var quot = r / nr;

            var tmp = nt; nt = t - quot * nt; t = tmp;
            tmp = nr; nr = r - quot * nr; r = tmp;
        }

        if (r > 1) throw new ArgumentException(nameof(a) + " is not convertible.");
        if (t < 0) t = t + n;
        return t;
    }


    public static string Hex(BigInteger n)
    {
        var s = n.ToString("X");
        while (s.Length > 1 && s.Substring(0, 1) == "0") s = s.Substring(1);
        if (s.Length == 1) s = "0" + s;
        return s;
    }

    public static BigInteger ConvertFrom(this string number, NumberFormat format, bool positiveOnly = true)
    {
        number = number.Replace(":", "").Replace(" ", "").Trim();
        switch (format)
        {
            case NumberFormat.Binary: return number.NewBigInteger2();
            case NumberFormat.Decimal:
                BigInteger B;
                if (!BigInteger.TryParse(number, out B)) B = 0;
                return B;
            case NumberFormat.Hexadecimal: return GetBig(number, positiveOnly);
            case NumberFormat.Base64: return GetBig(Convert.FromBase64String(number), positiveOnly);
        }

        return 0;
    }

    public static string ConvertTo(this BigInteger number, NumberFormat format)
    {
        switch (format)
        {
            case NumberFormat.Binary: return ToBinaryString(number);
            case NumberFormat.Decimal: return number.ToString();
            case NumberFormat.Hexadecimal: return Hex(number);
            case NumberFormat.Base64: return Convert.ToBase64String(number.BigToArray());
        }

        return "0";
    }

    /// <summary>
    /// Converts a non-negative integer to an octet string of a specified length.
    /// </summary>
    /// <param name="x">The integer to convert.</param>
    /// <param name="xLen">Length of output octets.</param>
    /// <param name="makeLittleEndian">If True little-endian converntion is followed, big-endian otherwise.</param>
    /// <returns></returns>
    public static byte[] ToByteArray(BigInteger x, int xLen, bool makeLittleEndian)
    {
        byte[] result = new byte[xLen];
        int index = 0;
        while ((x > 0) && (index < result.Length))
        {
            result[index++] = (byte)(x % 256);
            x >>= 8;
        }
        if (!makeLittleEndian)
            Array.Reverse(result);
        return result;
    }

    /// <summary>
    /// Converts a byte array to a non-negative integer.
    /// </summary>
    /// <param name="data">The number in the form of a byte array.</param>
    /// <param name="isLittleEndian">Endianness of the byte array.</param>
    /// <returns>An non-negative integer from the byte array of the specified endianness.</returns>
    public static BigInteger ToPositiveBigInteger(byte[] data, bool isLittleEndian)
    {
        BigInteger bi = 0,p=1;
        if (isLittleEndian)
        {
            for (int i = 0; i < data.Length; i++)
            {
                bi += p * data[i];
                p <<= 8;
            }
        }
        else
        {
            for (int i = 1; i <= data.Length; i++)
            {
                bi += p * data[data.Length - i];
                p <<= 8;
            }
        }
        return bi;
    }

    public static byte[] BigToArray(this BigInteger n)
    {
        byte[] data = n.ToByteArray();
        byte[] reversed = new byte[data.Length];
        Array.Copy(data, 0, reversed, 0, data.Length);
        Array.Reverse(reversed);
        return reversed;
    }
}