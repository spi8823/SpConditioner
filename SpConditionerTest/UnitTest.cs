using SpConditioner;

namespace SpConditionerTest
{
    [TestClass]
    public class UnitTest
    {
        private TestVariableAccessor variableAccessor = new TestVariableAccessor();

        [TestMethod]
        public void TestStatements()
        {
            void TestStatement(bool expected, string statement)
            {
                var result = StatementParser.ParseToBoolFunc(statement).Invoke(variableAccessor);
                Assert.AreEqual(expected, result, statement);
            }

            TestStatement(true, "T");
            TestStatement(false, "F");

            TestStatement(true, "T && T");
            TestStatement(false, "T && F");
            TestStatement(true, "T || F");
            TestStatement(false, "F || F");

            TestStatement(true, "One == 1");
            TestStatement(true, "One != 2");
            TestStatement(true, "Two == 1 + 1");
            TestStatement(false, "Three == 1 + 1");

            TestStatement(true, "Two == One + 1");
            TestStatement(true, "Six == Two * Three");
            TestStatement(false, "Five == Three + One");

            TestStatement(true, "Two == One + 1 && T");
            TestStatement(false, "One == One && F");
            TestStatement(true, "T && One == 1");
            TestStatement(false, "F && One == 1");

            TestStatement(true, "One < Two");
            TestStatement(false, "One > Two");
            TestStatement(true, "One - 1 < One");
            TestStatement(false, "Two * Three < Five");

            TestStatement(true, "T && T && T");
            TestStatement(false, "F && T && T");
            TestStatement(false, "T && F && T");
            TestStatement(false, "T && T && F");

            TestStatement(true, "(3 + 4) == 7");
            TestStatement(false, "(3 + 4) == 8");
            TestStatement(true, "(2 + 3) * 4 == 20");
            TestStatement(false, "(2 + 3) * 4 == 21");
            TestStatement(true, "2 * (3 + 4) == 14");
            TestStatement(false, "2 * (3 + 4) == 15");
            TestStatement(true, "18 == (3 + 3) * 3");
            TestStatement(false, "17 == (3 + 3) * 3");
            TestStatement(true, "4 + (2 + 6) * 5 == 4 * (5 + 6)");
            TestStatement(false, "4 + (1 + 6) * 5 == 4 * (5 + 6)");

            TestStatement(true, "(T || F) && T");
            TestStatement(false, "(T && F) && T");
            TestStatement(true, "F || (T || F)");
            TestStatement(false, "F || (T && F)");

            TestStatement(true, "(2 > 1) || F");
            TestStatement(false, "(1 > 2) || F");
            TestStatement(false, "(2 > 1 && F) || F");

            TestStatement(true, "(One + Two) == 3");
            TestStatement(true, "(One + Two) * Three == 9");
            TestStatement(false, "(One + Two) * Three == 8");
            TestStatement(true, "(One + Two) * Three == (Two * Three) + 3");
            TestStatement(false, "(One + Two) * Three == (Two * Three) + 2");
        }

        [TestMethod]
        public void TestConditionState()
        {
            var state = new ConditionState();
            void Check(bool expected)
            {
                var result = state.Check(variableAccessor);
                Assert.AreEqual(expected, result);
            }

            Check(true);

            // if true
            state.OnIf("One == 1");
            Check(true);
            // else
            state.OnElse();
            Check(false);
            // endif
            state.OnEndIf();
            Check(true);

            // if false
            state.OnIf("One == 2");
            Check(false);
            // else
            state.OnElse();
            Check(true);
            // endif
            state.OnEndIf();

            // if false
            state.OnIf("One == 2");
            // else if true
            state.OnElseIf("Two == 2");
            Check(true);
            // else
            state.OnElse();
            Check(false);
            // endif
            state.OnEndIf();

            // if false
            state.OnIf("One == 2");
            // else if true
            state.OnElseIf("One == 1");
            //     if false
            state.OnIf("Two == 1");
            Check(false);
            //     else if true
            state.OnElseIf("Two == 2");
            Check(true);
            //     else
            state.OnElse();
            Check(false);
            //     endif
            state.OnEndIf();
            // endif
            state.OnEndIf();

            Check(true);
        }
    }
}