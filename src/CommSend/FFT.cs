using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommSend
{
    public class Complex
    {
        private double real;
        private double imag;

        public Complex():this(0d, 0d) { }

        public Complex(double real):this(real, 0d) { }

        public Complex(double real, double imag)
        {
            this.real = real;
            this.imag = imag;
        }

        public double Real
        {
            get { return real; }
            set { real = value; }
        }
        public double Image
        {
            get { return imag; }
            set { imag = value; }
        }

        public static Complex operator +(Complex c1, Complex c2)
        {
            return new Complex(c1.real + c2.real, c1.imag + c2.imag);
        }

        public static Complex operator -(Complex c1, Complex c2)
        {
            return new Complex(c1.real - c2.real, c1.imag - c2.imag);
        }

        public static Complex operator *(Complex c1, Complex c2)
        {
            return new Complex(c1.real * c2.real - c1.imag * c2.imag, c1.imag * c2.real + c1.real * c2.imag);
        }

        public void Sub(double num)
        {
            if (num == 0)
            {
                throw new DivideByZeroException();
            }
            this.real /= num;
            this.imag /= num;
        }

        public override string ToString()
        {
            if (Math.Abs(Real) <= double.Epsilon && Math.Abs(Image) <= double.Epsilon)
            {
                return "0";
            }
            if (Math.Abs(Real) <= double.Epsilon)
            {
                if (Image != 1 && Image != -1)
                {
                    return string.Format("{0}i", Image);
                }
                if (Image == 1)
                {
                    return "i";
                }
                if (Image == -1)
                {
                    return "-i";
                }
            }
            if (Math.Abs(Image) <= double.Epsilon)
            {
                return Real.ToString();
            }
            if (Image < 0)
            {
                return string.Format("{0}-{1}i", Real, -Image);
            }
            return string.Format("{0}+{1}i", Real, Image);
        }

        public double ToModul()
        {
            return Math.Sqrt(real * real + imag * imag);
        }
    }
    public class FFTrans
    {
        public static void BitRP(Complex[] origin, int n)
        {
            int i, j, a, b, p;
            for (i=1, p=0; i <n; i *=2)
            {
                p++;
            }
            for (i=0;i<n;i++)
            {
                a = i;
                b = 0;
                for (j=0;j<p;j++)
                {
                    b = b * 2 + a % 2;
                    a /= 2;
                }
                if (b > i)
                {
                    Complex t = origin[i];
                    origin[i] = origin[b];
                    origin[b] = t;
                }
            }
        }

        private static int Trans(Complex[] origin, double arg)
        {
            //Complex[] result = new Complex[origin.Length];
            int n = 2;
            while (n <= origin.Length)
            {
                n *= 2;
            }
            n /= 2;
            Complex[] w_origin = new Complex[n / 2];
            Complex t_origin, u_origin;
            int m, k, j, t, i1, i2;
            BitRP(origin, n);
           // origin.CopyTo(result, 0);
            t_origin = new Complex(Math.Cos(arg/(double)n), Math.Sin(arg/n));
            w_origin[0] = new Complex(1);
            for (j = 1; j < n / 2; j++)
            {
                w_origin[j] = t_origin * w_origin[j - 1];
            }
            for (m = 2; m <= n; m *= 2)
            {
                for (k = 0; k < n; k += m)
                {
                    for (j = 0; j < m / 2; j++)
                    {
                        i1 = k + j;
                        i2 = i1 + m / 2;
                        t = n * j / m;
                        t_origin = origin[i2] * w_origin[t];
                        u_origin = origin[i1];
                        origin[i1] = u_origin + t_origin;
                        origin[i2] = u_origin - t_origin;
                    }
                }
            }
            return n;
        }

        public static int FFT(Complex[] origin)
        {
            double arg = -2 * Math.PI;
            return Trans(origin, arg);
        }

        public static Complex[] IFFT(Complex[] origin)
        {
            double arg = 2 * Math.PI;
            int n = Trans(origin, arg);
            for (int i = 0;i < n;i++)
            {
                origin[i].Sub(1.0 * n);
            }
            return origin;
        }

        public static Complex[] LowPassFilter(Complex[] origin, double thero)
        {
            Complex[] result = new Complex[origin.Length];
            for (int i = 0;i<origin.Length;i++)
            {
                if ((i < origin.Length * thero )|| (i > origin.Length * (1 - thero)))
                {
                    result[i] = origin[i];
                }
                else
                {
                    result[i] = new Complex();
                }
            }
            return result;
        }

        public static Complex[] HighPassFilter(Complex[] origin, double thero)
        {
            Complex[] result = new Complex[origin.Length];
            for (int i = 0; i < origin.Length; i++)
            {
                if ((i >= origin.Length * thero) && (i <= origin.Length * (1 - thero)))
                {
                    result[i] = origin[i];
                }
                else
                {
                    result[i] = new Complex();
                }
            }
            return result;
        }

        public static List<float> LowPassTrans(List<float> origin, double thero)
        {
            List<float> result = new List<float>();
            Complex[] cache = new Complex[origin.Count];
            for (int i =0;i<origin.Count;i++)
            {
                cache[i] = new Complex(origin[i]);
            }
            FFT(cache);
            Complex[] cache_low = LowPassFilter(cache, thero);
            IFFT(cache_low);
            for (int i=0;i< cache_low.Length;i++)
            {
                result.Add((float)cache_low[i].Real);
            }
            return result;
        }

        public static List<float> HighPassTrans(List<float> origin, double thero)
        {
            List<float> result = new List<float>();
            Complex[] cache = new Complex[origin.Count];
            for (int i = 0; i < origin.Count; i++)
            {
                cache[i] = new Complex(origin[i]);
            }
            FFT(cache);
            Complex[] cache_low = HighPassFilter(cache, thero);
            IFFT(cache_low);
            for (int i = 0; i < cache_low.Length; i++)
            {
                result.Add((float)cache_low[i].Real);
            }
            return result;
        }
    }
}
