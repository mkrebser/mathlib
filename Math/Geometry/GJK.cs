using System.Collections;


/// <summary>
/// struct used to compute gjk intersection
/// </summary>
public struct GJK
{
    Vector3d v;
    Vector3d a, b, c, d;
    int n;

    //uses GJK algorithm
    //see https://mollyrocket.com/849 for a video tutorial

    //i translated this c++ implemtation
    //http://vec3.ca/gjk/implementation/

    /// <summary>
    /// Returns true if input convex polyhedrans intersect. The input points should all be extremes
    /// </summary>
    /// <param name="points1"></param>
    /// <param name="points2"></param>
    /// <returns></returns>
    public bool intersect(Vector3d[] points1, Vector3d[] points2)
    {
        v = new Vector3d(1, 0, 0); //some arbitrary starting vector

        c = support(points1, points2, v);
        if (Vector3d.Dot(c, v) < 0)
            return false;

        v = -c;
        b = support(points1, points2, v);

        if (Vector3d.Dot(b, v) < 0)
            return false;

        v = cross_aba(c - b, -b);
        n = 2;

        for (int iterations = 0; iterations < 32; iterations++)
        {
            Vector3d a = support(points1, points2, v);

            if (Vector3d.Dot(a, v) < 0)
                return false;

            if (update(a))
                return true;
        }

        //out of iterations, be conservative
        return true;
    }

    /// <summary>
    /// Returns true if input convex polyhedran and sphere intersect. The input points should all be extremes
    /// </summary>
    /// <param name="points1"></param>
    /// <param name="points2"></param>
    /// <returns></returns>
    public bool intersect(Vector3d[] points1, Sphere points2)
    {
        v = new Vector3d(1, 0, 0); //some arbitrary starting vector

        c = support(points1, points2, v);
        if (Vector3d.Dot(c, v) < 0)
            return false;

        v = -c;
        b = support(points1, points2, v);

        if (Vector3d.Dot(b, v) < 0)
            return false;

        v = cross_aba(c - b, -b);
        n = 2;

        for (int iterations = 0; iterations < 32; iterations++)
        {
            Vector3d a = support(points1, points2, v);

            if (Vector3d.Dot(a, v) < 0)
                return false;

            if (update(a))
                return true;
        }

        //out of iterations, be conservative
        return true;
    }

    /// <summary>
    /// Returns true if input convex polyhedran and capsule intersect. The input points should all be extremes
    /// </summary>
    /// <param name="points1"></param>
    /// <param name="points2"></param>
    /// <returns></returns>
    public bool intersect(Vector3d[] points1, Capsule points2)
    {
        v = new Vector3d(1, 0, 0); //some arbitrary starting vector

        c = support(points1, points2, v);
        if (Vector3d.Dot(c, v) < 0)
            return false;

        v = -c;
        b = support(points1, points2, v);

        if (Vector3d.Dot(b, v) < 0)
            return false;

        v = cross_aba(c - b, -b);
        n = 2;

        for (int iterations = 0; iterations < 32; iterations++)
        {
            Vector3d a = support(points1, points2, v);

            if (Vector3d.Dot(a, v) < 0)
                return false;

            if (update(a))
                return true;
        }

        //out of iterations, be conservative
        return true;
    }

    Vector3d support(Vector3d[] p1, Vector3d[] p2, Vector3d direction)
    {
        //TODO: better support function
        double m1 = double.MinValue, m2 = double.MinValue;
        Vector3d mp1 = Vector3d.zero, mp2 = Vector3d.zero;
        for (int i = 0; i < p1.Length; i++)
        {
            double val = Vector3d.Dot(p1[i], direction);
            if (val > m1)
            {
                m1 = val;
                mp1 = p1[i];
            }
        }
        for (int i = 0; i < p2.Length; i++)
        {
            double val = Vector3d.Dot(p2[i], -direction);
            if (val > m2)
            {
                m2 = val;
                mp2 = p2[i];
            }
        }
        return mp1 - mp2;
    }

    Vector3d support(Vector3d[] p1, Sphere p2, Vector3d direction)
    {
        //TODO: better support function
        double m1 = double.MinValue;
        Vector3d mp1 = Vector3d.zero, mp2 = Vector3d.zero;
        for (int i = 0; i < p1.Length; i++)
        {
            double val = Vector3d.Dot(p1[i], direction);
            if (val > m1)
            {
                m1 = val;
                mp1 = p1[i];
            }
        }
        mp2 = p2.Center + -direction.normalized * p2.Radius;
        return mp1 - mp2;
    }

    Vector3d support(Vector3d[] p1, Capsule p2, Vector3d direction)
    {
        //TODO: better support function
        double m1 = double.MinValue;
        Vector3d mp1 = Vector3d.zero, mp2 = Vector3d.zero;
        for (int i = 0; i < p1.Length; i++)
        {
            double val = Vector3d.Dot(p1[i], direction);
            if (val > m1)
            {
                m1 = val;
                mp1 = p1[i];
            }
        }

        //capsule. is direction towards top of capsule?
        if (Vector3d.Dot(p2.Up, direction) > 0)
            mp2 = p2.Up + -direction.normalized * p2.Radius;
        //other wise bottom
        else
            mp2 = p2.Down + -direction.normalized * p2.Radius;

        return mp1 - mp2;
    }

    Vector3d cross_aba(Vector3d a, Vector3d b)
    {
        return Vector3d.Cross(Vector3d.Cross(a, b), a);
    }

    bool update(Vector3d a)
    {
        if (n == 2)
        {
            Vector3d ao = -a;

            Vector3d ab = b - a;
            Vector3d ac = c - a;

            Vector3d abc = Vector3d.Cross(ab, ac);

            Vector3d abp = Vector3d.Cross(ab, abc);

            if (Vector3d.Dot(abp, ao) > 0)
            {
                c = b;
                b = a;

                v = cross_aba(ab, ao);

                return false;
            }

            Vector3d acp = Vector3d.Cross(abc, ac);

            if (Vector3d.Dot(acp, ao) > 0)
            {
                b = a;
                v = cross_aba(ac, ao);

                return false;
            }

            if (Vector3d.Dot(abc, ao) > 0)
            {
                d = c;
                c = b;
                b = a;

                v = abc;
            }
            else
            {
                d = b;
                b = a;

                v = -abc;
            }

            n = 3;

            return false;
        }
        else if (n == 3)
        {
            Vector3d ao = -a;

            Vector3d ab = b - a;
            Vector3d ac = c - a;
            Vector3d ad = d - a;

            Vector3d abc = Vector3d.Cross(ab, ac);
            Vector3d acd = Vector3d.Cross(ac, ad);
            Vector3d adb = Vector3d.Cross(ad, ab);

            Vector3d tmp;

            const int over_abc = 0x1;
            const int over_acd = 0x2;
            const int over_adb = 0x4;

            int plane_tests =
                (Vector3d.Dot(abc, ao) > 0 ? over_abc : 0) |
                (Vector3d.Dot(acd, ao) > 0 ? over_acd : 0) |
                (Vector3d.Dot(adb, ao) > 0 ? over_adb : 0);

            switch (plane_tests)
            {
                case 0:
                    return true;

                case over_abc:
                    goto check_one_face;

                case over_acd:

                    b = c;
                    c = d;

                    ab = ac;
                    ac = ad;

                    abc = acd;

                    goto check_one_face;

                case over_adb:

                    c = b;
                    b = d;

                    ac = ab;
                    ab = ad;

                    abc = adb;

                    goto check_one_face;

                case over_abc | over_acd:
                    goto check_two_faces;

                case over_acd | over_adb:

                    tmp = b;
                    b = c;
                    c = d;
                    d = tmp;

                    tmp = ab;
                    ab = ac;
                    ac = ad;
                    ad = tmp;

                    abc = acd;
                    acd = adb;

                    goto check_two_faces;

                case over_adb | over_abc:

                    tmp = c;
                    c = b;
                    b = d;
                    d = tmp;

                    tmp = ac;
                    ac = ab;
                    ab = ad;
                    ad = tmp;

                    acd = abc;
                    abc = adb;

                    goto check_two_faces;

                default:
                    return true;
            }

            check_one_face:

            if (Vector3d.Dot(Vector3d.Cross(abc, ac), ao) > 0)
            {

                b = a;

                v = cross_aba(ac, ao);

                n = 2;

                return false;
            }

            check_one_face_part_2:

            if (Vector3d.Dot(Vector3d.Cross(ab, abc), ao) > 0)
            {

                c = b;
                b = a;

                v = cross_aba(ab, ao);

                n = 2;

                return false;
            }

            d = c;
            c = b;
            b = a;

            v = abc;

            n = 3;

            return false;

            check_two_faces:

            if (Vector3d.Dot(Vector3d.Cross(abc, ac), ao) > 0)
            {
                b = c;
                c = d;

                ab = ac;
                ac = ad;

                abc = acd;

                goto check_one_face;
            }

            goto check_one_face_part_2;
        }
        else
            throw new System.Exception("wtf");
    }
}

