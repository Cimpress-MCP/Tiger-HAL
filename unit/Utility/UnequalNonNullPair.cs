// <copyright file="UnequalNonNullPair.cs" company="Cimpress, Inc.">
//   Copyright 2020 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License") –
//   you may not use this file except in compliance with the License.
//   You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.
// </copyright>

namespace Test.Utility
{
    static class UnequalNonNullPair
    {
        public static void Deconstruct<T>(this UnequalNonNullPair<T> pair, out T left, out T right)
            where T : class
        {
            left = pair.Left;
            right = pair.Right;
        }
    }
}
