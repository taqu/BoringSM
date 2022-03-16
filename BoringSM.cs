/**
# License
This software is distributed under two licenses, choose whichever you like.

## MIT License
Copyright (c) 2022 Takuro Sakai

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## Public Domain
This is free and unencumbered software released into the public domain.

Anyone is free to copy, modify, publish, use, compile, sell, or
distribute this software, either in source code form or as a compiled
binary, for any purpose, commercial or non-commercial, and by any
means.

In jurisdictions that recognize copyright laws, the author or authors
of this software dedicate any and all copyright interest in the
software to the public domain. We make this dedication for the benefit
of the public at large and to the detriment of our heirs and
successors. We intend this dedication to be an overt act of
relinquishment in perpetuity of all present and future rights to this
software under copyright law.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS BE LIABLE FOR ANY CLAIM, DAMAGES OR
OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.

For more information, please refer to <http://unlicense.org>
*/
using System;
using System.Diagnostics;
using System.Reflection;

namespace BoringSM
{
    /// <summary>
    /// Minimal State Machine
    /// </summary>
    /// <typeparam name="ParentType">The parent who owns a state machine</typeparam>
    /// <typeparam name="StateType">The enum type of states</typeparam>
    /// <example>
    /// This shows how to define a state machine. Use safixes, "_Init", "_Proc", "_Term", to define eache state's methods.
    /// <code>
    /// class SampleSM
    /// {
    ///     private enum State
    ///     {
    ///         Init,
    ///         State0,
    ///         State1,
    ///     };
    ///     BoringSM.BoringSM<SampleSM, State> state_;
    ///     public SampleSM()
    ///     {
    ///         state_ = new BoringSM<SampleSM, State>(this);
    ///     }
    ///     
    ///     public void update()
    ///     {
    ///         state_.update();
    ///     }
    ///     
    ///     public bool IsEnd
    ///     {
    ///         get { return (int)State.State1 == state_.get(); }
    ///     }
    ///     
    ///     private void Init_Init()
    ///     {
    ///         state_.set(State.State0); //Chaining inits is acceptable.
    ///     }
    ///     
    ///     //private void Init_Proc(){} //Not totally necessary
    ///     //private void Init_Term(){} //Not totally necessary
    ///     
    ///     //private void State0_Init(){} //Not totally necessary
    ///     private void State0_Proc()
    ///     {
    ///         state_.set(State.State1);
    ///     }
    ///     private void State0_Term()
    ///     {
    ///     }
    /// };
    /// </code>
    /// Use in outer like below.
    /// <code>
    /// SampleSM sample = new SampleSM();
    /// while(!sample.IsEnd){
    ///     sample.update();
    /// }
    /// </code>
    /// </example>
    public class BoringSM<ParentType, StateType> where StateType : struct, IConvertible
    {
        private struct StateInfo
        {
            public Action<ParentType> init_;
            public Action<ParentType> proc_;
            public Action<ParentType> term_;
        };

        private ParentType parent_;
        private StateInfo[] stateInfos_;
        private int prev_;
        private int current_;

        public BoringSM(ParentType parent)
        {
            Debug.Assert(null != parent);
            parent_ = parent;
            string[] stateNames = Enum.GetNames(typeof(StateType));
            stateInfos_ = new StateInfo[stateNames.Length];

            // Create delegates
            Type parentType = typeof(ParentType);
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            for(int i = 0; i < stateNames.Length; ++i) {
                int end = stateNames[i].Length;
                stringBuilder.Length = 0;
                stateInfos_[i].init_ = createDelegate(parentType, stringBuilder.Append(stateNames[i]).Append("_Init").ToString());
                stateInfos_[i].proc_ = createDelegate(parentType, stringBuilder.Remove(end, 5).Append("_Proc").ToString());
                stateInfos_[i].term_ = createDelegate(parentType, stringBuilder.Remove(end, 5).Append("_Term").ToString());
            }
            init(0);
        }

        /// <summary>
        /// Get the current state as int
        /// </summary>
        /// <returns></returns>
        public int get()
        {
            return prev_;
        }

        /// <summary>
        /// Set the initial state as int
        /// </summary>
        /// <param name="state"></param>
        public void init(int state)
        {
            Debug.Assert(0 <= state && state < stateInfos_.Length);
            prev_ = -1;
            current_ = state;
        }

        /// <summary>
        /// Set the initial state as enum
        /// </summary>
        /// <param name="state"></param>
        public void init(StateType state)
        {
            prev_ = -1;
            current_ = Convert.ToInt32(state);
            Debug.Assert(0 <= current_ && current_ < stateInfos_.Length);
        }

        /// <summary>
        /// Set the next state as int
        /// </summary>
        /// <param name="state"></param>
        public void set(int state)
        {
            Debug.Assert(0 <= state && state < stateInfos_.Length);
            current_ = state;

        }

        /// <summary>
        /// Set the next state as enum
        /// </summary>
        /// <param name="state"></param>
        public void set(StateType state)
        {
            current_ = Convert.ToInt32(state);
            Debug.Assert(0 <= current_ && current_ < stateInfos_.Length);
        }

        /// <summary>
        /// Update the internal state
        /// </summary>
        public void update()
        {
            while(prev_ != current_) { //Loop until the two have the same value.
                // Call termination
                if(0<=prev_ && null != stateInfos_[prev_].term_) {
                    stateInfos_[prev_].term_(parent_);
                }
                prev_ = current_;
                // Call initialization
                if(null != stateInfos_[current_].init_) {
                    stateInfos_[current_].init_(parent_);
                }
            }
            if(null != stateInfos_[current_].proc_){
                stateInfos_[current_].proc_(parent_);
            }
        }

        private static Action<ParentType> createDelegate(Type type, string method)
        {
            MethodInfo methodInfo = type.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            if(null == methodInfo) {
                return null;
            }
            return Delegate.CreateDelegate(typeof(Action<ParentType>), methodInfo) as Action<ParentType>;
        }
    };
}
