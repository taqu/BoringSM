# BoringSM
A minimal C# state machine.

# Usage

Use safixes, "_Init", "_Proc", "_Term", for each states, but it's not necessary.

```csharp
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

SampleSM sample = new SampleSM();
while(!sample.IsEnd){
    sample.update();
}
```

# Limitations

- Not support multithreading.
- Not support invalid use like, "pass null to the constructor", "pass an invalid state to the set function".

# LICENSE
This software is distributed under two licenses, choose whichever you like.

1. MIT License
2. Public Domain

