using System;
using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public struct Polynomial
{
    List<float> m_coefficients;
    public List<float> coefficients
    {
        get
        {
            while (m_coefficients.Count > 0 && m_coefficients.Last() == 0)
                m_coefficients.RemoveAt(m_coefficients.Count - 1);
            return m_coefficients;
        }
    }

    public float this[int i]
    {
        get
        {
            if (i < coefficients.Count) return coefficients[i];
            return 0;
        }
        set
        {
            if (i < coefficients.Count)
            {
                m_coefficients[i] = value;
                return;
            }
            else
                for (int j = coefficients.Count; j < i; j++)
                    m_coefficients.Add(0f);
            m_coefficients.Add(value);
        }
    }

    public int degree
    {
        get
        {
            return coefficients.Count - 1;
        }
    }

    public Polynomial(params float[] _coefficients)
    {
        m_coefficients = new List<float>(_coefficients);
    }

    public override string ToString()
    {
        List<string> terms = new List<string>();
        for (int i = degree; i >= 0; i--)
            if (this[i] != 0)
                terms.Add(this[i] + "x^" + i);

        for (int i = 0; i < terms.Count; i++)
        {
            if (terms[i].StartsWith("0x"))
            {
                terms.RemoveAt(i);
                i--;
            }
            else if (terms[i].StartsWith("1x") && !terms[i].EndsWith("^0"))
            {
                terms[i] = terms[i].Remove(0, 1);
                i--;
            }
            else if (terms[i].EndsWith("^0"))
            {
                terms[i] = terms[i].Split(char.Parse("x"))[0];
                i--;
            }
            else if (terms[i].EndsWith("^1"))
            {
                terms[i] = terms[i].Split(char.Parse("^"))[0];
            }
        }
        string polyString = String.Join(" + ", terms);
        if (polyString == "") polyString = "0";

        return polyString;
    }

    public static implicit operator Func<float, float>(Polynomial p) => (float x) => p.Evaluate(x);

    public static Polynomial operator +(Polynomial p, Polynomial q)
    {
        Polynomial r = new Polynomial(0);
        for (int i = 0; i <= Math.Max(p.degree, q.degree); i++)
            r[i] = p[i] + q[i];
        return r;
    }
    public static Polynomial operator *(Polynomial p, Polynomial q)
    {
        Polynomial r = new Polynomial(0);
        for (int i = 0; i <= p.degree; i++)
            for (int j = 0; j <= q.degree; j++)
                r[i + j] += p[i] * q[j];
        return r;
    }
    public static Polynomial operator *(float x, Polynomial p) => new Polynomial(x) * p;
    public static Polynomial operator *(Polynomial p, float x) => new Polynomial(x) * p;
    public static Polynomial operator /(Polynomial p, float x) => new Polynomial(1 / x) * p;

    public float Evaluate(float x)
    {
        if (x == 0) return this[0];

        float y = 0;
        for (int i = 0; i < coefficients.Count; i++)
            y += this[i] * Mathf.Pow(x, i);

        return y;
    }

    // Lagrange interpolation
    public static Polynomial Interpolate(params Vector2[] absiccae)
    {
        Polynomial[] bases = new Polynomial[absiccae.Length];
        int k = absiccae.Length;             // This is actually k + 1 in typical notation.
        for (int i = 0; i < k; i++)
        {
            Polynomial absicca = new Polynomial(1);
            for (int j = 0; j < k; j++)
            {
                if (i == j) continue;
                absicca *= new Polynomial(-absiccae[j].x, 1);
            }
            bases[i] = absicca;
        }

        Polynomial interpolation = new Polynomial(0);
        for (int i = 0; i < k; i++)
            interpolation += absiccae[i].y * bases[i] / bases[i].Evaluate(absiccae[i].x);

        return interpolation;
    }

    // returns the indefinite integral of *this*, assumes c = 0
    public void Integrate()
    {
        for (int i = coefficients.Count; i > 0; i--)
            coefficients[i] = coefficients[i - 1] / i;
        coefficients[0] = 0;
    }
    public static Polynomial Integrate(Polynomial integrand) { integrand.Integrate(); return integrand; }
    public Polynomial integral
    {
        get
        {
            Polynomial p = new Polynomial(coefficients.ToArray());
            p.Integrate();
            return p;
        }
    }

    public void Differentiate()
    {
        for (int i = 0; i < degree - 1; i++)
            coefficients[i] = coefficients[i + 1] * (i + 1);
        coefficients[degree - 1] = 0;
    }
    public static Polynomial Differentiate(Polynomial derivand) { derivand.Differentiate(); return derivand; }
    public Polynomial derivative
    {
        get
        {
            Polynomial p = new Polynomial(coefficients.ToArray());
            p.Differentiate();
            return p;
        }
    }
}
