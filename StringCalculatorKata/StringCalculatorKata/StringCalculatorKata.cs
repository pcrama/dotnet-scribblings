using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using FluentAssertions;

// Inspired by https://osherove.com/tdd-kata-1

namespace StringCalculatorKata
{
    [TestFixture]
    public class StringCalculatorKata
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void StringCalculator_Add__EmptyString__Returns0()
        {
            StringCalculator.Add("").Should().Be(0);
        }

        [Test]
        public void StringCalculator_Add__null__Returns0()
        {
            StringCalculator.Add(null).Should().Be(0);
        }

        [Test]
        public void StringCalculator_Add__1__Returns1()
        {
            StringCalculator.Add("1").Should().Be(1);
        }

        [Test]
        public void StringCalculator_Add__2__Returns2()
        {
            StringCalculator.Add("2").Should().Be(2);
        }

        [Test]
        public void StringCalculator_Add__1_2__Returns3()
        {
            StringCalculator.Add("1,2").Should().Be(3);
        }

        [Test]
        public void StringCalculator_Add__3_4__Returns7()
        {
            StringCalculator.Add("3,4").Should().Be(7);
        }

        [Test]
        public void StringCalculator_Add__5comma__ThrowsFormatException()
        {
            Action act = () => StringCalculator.Add("5,");

            act.Should().Throw<FormatException>();
        }

        [Test]
        public void StringCalculator_Add__abcd__ThrowsFormatException()
        {
            Action act = () => StringCalculator.Add("abcd");

            act.Should().Throw<FormatException>();
        }

        [Test]
        public void StringCalculator_Add__1_2_3__Returns6()
        {
            StringCalculator.Add("1,2,3").Should().Be(6);
        }

        [Test]
        public void StringCalculator_Add__1_2_3_4__Returns10()
        {
            StringCalculator.Add("1,2,3,4").Should().Be(10);
        }

        [Test]
        public void StringCalculator_Add__1_newline_2_3__Returns6()
        {
            StringCalculator.Add("1\n2,3").Should().Be(6);
        }

        [Test]
        public void StringCalculator_Add__1_comma_newline_2_3__ThrowsFormatException()
        {
            Action act = () => StringCalculator.Add("1,\n2,3");

            act.Should().Throw<FormatException>();
        }

        [Test]
        public void StringCalculator_Add__1_newline_comma_2_3__ThrowsFormatException()
        {
            Action act = () => StringCalculator.Add("1\n,2,3");

            act.Should().Throw<FormatException>();
        }

        [Test]
        public void StringCalculator_Add__semicolondelimiter_1_2_3__Returns6()
        {
            StringCalculator.Add("//;\n1;2;3").Should().Be(6);
        }

        [Test]
        public void StringCalculator_Add__semicolondelimiter_1_2_newline_3_4__Returns10()
        {
            StringCalculator.Add("//;\n1;2\n3;4").Should().Be(10);
        }

        [Test]
        public void StringCalculator_Add__semicolondelimiter_ThenEmpty__Returns0()
        {
            StringCalculator.Add("//;\n").Should().Be(0);
        }

        [Test]
        public void StringCalculator_Add__semicolondelimiter_ThenNotEvenNewline__Returns0()
        {
            StringCalculator.Add("//;").Should().Be(0);
        }

        [Test]
        public void StringCalculator_Add__semicolondelimiter_Then1__Returns1()
        {
            StringCalculator.Add("//;\n1").Should().Be(1);
        }

        [Test]
        public void StringCalculator_Add__NoDelimiter__ThrowsFormatException()
        {
            Action act = () => StringCalculator.Add("//");

            act.Should().Throw<FormatException>().Where(e => e.Message.Contains("delimiter")).Where(e => e.Message.Contains("'//'"));
        }

        [Test]
        public void StringCalculator_Add__Minus1__ThrowsArgumentOutOfRangeException()
        {
            Action act = () => StringCalculator.Add("-1");

            act.Should().Throw<ArgumentOutOfRangeException>().Where(e => e.Message.Contains("Negatives not allowed")).Where(e => e.Message.Contains("-1"));
        }

        [Test]
        public void StringCalculator_Add__Minus1_2_Minus3__ThrowsArgumentOutOfRangeException()
        {
            Action act = () => StringCalculator.Add("//;\n-1\n2;-3");

            act.Should().
                Throw<ArgumentOutOfRangeException>().
                Where(e => e.Message.Contains("Negatives not allowed")).
                Where(e => e.Message.Contains("-1")).
                Where(e => e.Message.Contains("-3"));
        }

        [Test]
        public void StringCalculator_Add__1_1000_1001_1002_2__Returns1003() /* Numbers above 1000 are ignored */
        {
            StringCalculator.Add("1,1000,1001,1002,2").Should().Be(1003);
        }

        [Test]
        public void StringCalculator_Add__squareBracketdelimiter_1_2_3__Returns6()
        {
            StringCalculator.Add("//[\n1[2[3").Should().Be(6);
        }

        [Test]
        public void StringCalculator_Add__MultiCharDelimiter_1_2_3__Returns6()
        {
            StringCalculator.Add("//[***]\n1***2***3").Should().Be(6);
        }

        [Test]
        public void StringCalculator_Add__MultiCharDelimiterAndNewline_1_2_3__Returns6()
        {
            StringCalculator.Add("//[***]\n1***2\n3").Should().Be(6);
        }

        [Test]
        public void StringCalculator_Add__ManyMultiCharDelimiter_1_2_3_4__Returns10()
        {
            StringCalculator.Add("//[***][;;][,]\n1***2;;3,4").Should().Be(10);
        }
    }

    public class StringCalculator
    {
        private static readonly string DELIMITER_MARKER = "//";
        private static readonly int DELIMITER_MARKER_LINE_LENGTH = DELIMITER_MARKER.Length + 1 /* separator char */ + 1 /* new line */;

        public static int Add(string numbers)
        {
            if (!String.IsNullOrEmpty(numbers) && numbers.StartsWith(DELIMITER_MARKER))
            {
                if (numbers.Length > DELIMITER_MARKER.Length)
                {
                    if (numbers.Length <= DELIMITER_MARKER_LINE_LENGTH)
                    {
                        // covers "//," and "//,\n"
                        return 0;
                    }
                    else
                    {
                        var lines = numbers.Split('\n');
                        var prefix_to_drop = lines[0].Length + 1 /* newline character */;
                        lines[0] = lines[0].Substring(DELIMITER_MARKER.Length); // Drop "//" prefix
                        if (lines[0].Length > 0 && lines[0][0] == '[')
                        {
                            if (lines[0] == "[")
                            {
                                return AddWithDelimiter(numbers.Substring(prefix_to_drop), '[');
                            }
                            else
                            {
                                var delimiters = lines[0].Split("][");
                                delimiters[0] = delimiters[0].Substring(1); // Drop '[' prefix
                                delimiters[^1] = delimiters[^1][0..^1]; // Drop ']' suffix
                                return AddArray(numbers.Substring(prefix_to_drop).Split(delimiters, System.StringSplitOptions.RemoveEmptyEntries));
                            }
                        }
                        else
                        {
                            return AddWithDelimiter(
                                numbers.Substring(prefix_to_drop),
                                numbers[DELIMITER_MARKER.Length]);
                        }
                    }
                }
                else
                {
                    throw new FormatException($"Expected delimiter after '{DELIMITER_MARKER}'");
                }
            }
            else
            {
                return AddWithDelimiter(numbers, ',');
            }
        }

        private static int AddArray(string[] numbers)
        {
            var result = numbers.
                SelectMany(p => p.Split('\n')).
                Select(p => int.Parse(p)).
                Select(x => ValidateNumber(x)).
                Aggregate(Either<IEnumerable<int>, int>.MakeRight(0),
                    (acc, x) => Combine(acc, x));
            if (result.IsLeft)
            {
                throw new ArgumentOutOfRangeException($"Negatives not allowed: {String.Join(',', result.Left.Select(x => x.ToString()))}");
            }
            else
            {
                return result.Right;
            }

        }
        private static int AddWithDelimiter(string numbers, char delimiter_char)
        {
            if (String.IsNullOrEmpty(numbers))
            {
                return 0;
            }
            return AddArray(numbers.Split(delimiter_char));
        }

        private static Either<IEnumerable<int>, int> Combine(Either<IEnumerable<int>, int> acc, Either<List<int>, int> x)
        {
            if (x.IsLeft)
            {
                return x.MapLeft(lx => acc.Project(la => la.Concat(lx), _ => lx));
            }
            else
            {
                /* x.IsRight -> acc defines Left/Right of result */
                return acc.Dimap(la => la,
                                 ra => x.Project(lx => ra /* should never happen, this case is already handled */,
                                                 rx => ra + rx));
            }
        }

        private static Either<List<int>, int> ValidateNumber(int x)
        {
            if (x < 0)
            {
                return Either<List<int>, int>.MakeLeft(new List<int> { x });
            }
            else
            {
                return Either<List<int>, int>.MakeRight(x > 1000 ? 0 : x);
            }
        }
 
        private class Either<L, R>
        {
            public readonly L Left;
            public readonly R Right;
            public readonly bool IsLeft;
            public bool IsRight { get => !IsLeft; }

            private Either(bool isLeft, L left, R right)
            {
                IsLeft = isLeft;
                Left = left;
                Right = right;
            }

            public static Either<L, R> MakeLeft(L left)
            {
                return new Either<L, R>(true, left, default);
            }

            public static Either<L, R> MakeRight(R right)
            {
                return new Either<L, R>(false, default, right);
            }

            public Either<M, R> MapLeft<M>(Func<L, M> leftFunc)
            {
                if (IsLeft)
                {
                    return Either<M, R>.MakeLeft(leftFunc(Left));
                }
                else
                {
                    return Either<M, R>.MakeRight(Right);
                }
            }

            public T Project<T>(Func<L, T> leftFunc, Func<R, T> rightFunc)
            {
                if (IsLeft)
                {
                    return leftFunc(Left);
                }
                else
                {
                    return rightFunc(Right);
                }
            }

            public Either<M, S> Dimap<M, S>(Func<L, M> leftFunc, Func<R, S> rightFunc)
            {
                if (IsLeft)
                {
                    return Either<M, S>.MakeLeft(leftFunc(Left));
                }
                else
                {
                    return Either<M, S>.MakeRight(rightFunc(Right));
                }
            }
        }
    }
}