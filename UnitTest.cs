using NUnit.Framework;

namespace BoringSM
{
    public class Tests
    {
        private class SampleSM
        {
            private enum State
            {
                Init,
                State0,
                State1,
            };
            BoringSM.BoringSM<SampleSM, State> state_;

            public SampleSM()
            {
                state_ = new BoringSM<SampleSM, State>(this);
            }

            public void update()
            {
                state_.update();
            }

            public bool IsEnd
            {
                get { return (int)State.State1 == state_.get();}
            }

            private void Init_Init()
            {
                state_.set(State.State0); //Chaining inits is acceptable.
            }

            //private void Init_Proc(){} //Not totally necessary
            //private void Init_Term(){} //Not totally necessary

            //private void State0_Init(){} //Not totally necessary
            private void State0_Proc()
            {
                state_.set(State.State1);
            }
            private void State0_Term()
            {
            }
        };

        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Test()
        {
            SampleSM sample = new SampleSM();
            while(!sample.IsEnd) {
                sample.update();
            }
            Assert.Pass();
        }
    }
}