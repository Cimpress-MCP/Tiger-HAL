// <copyright file="Relations.cs" company="Cimpress, Inc.">
//   Copyright 2018 Cimpress, Inc.
//
//   Licensed under the Apache License, Version 2.0 (the "License");
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

namespace Tiger.Hal
{
    /// <summary>A subset of the IANA-defined link relations.</summary>
    public static class Relations
    {
        /// <summary>
        /// The target URI points to a resource which represents
        /// the collection resource for the context IRI.
        /// </summary>
        public const string Collection = "collection";

        /// <summary>
        /// Refers to a resource containing the most recent item(s) in
        /// a collection of resources.
        /// </summary>
        public const string Current = "current";

        /// <summary>
        /// Refers to a resource whose available representations are byte-for-byte
        /// identical with the corresponding representations of the context URI.
        /// </summary>
        public const string Duplicate = "duplicate";

        /// <summary>
        /// A URI that refers to the furthest preceding resource
        /// in a series of resources.
        /// </summary>
        public const string First = "first";

        /// <summary>Refers to an index.</summary>
        public const string Index = "index";

        /// <summary>
        /// The target URI points to a resource that is a member
        /// of the collection represented by the context URI.
        /// </summary>
        public const string Item = "item";

        /// <summary>
        /// A URI that refers to the furthest following resource
        /// in a series of resources.
        /// </summary>
        public const string Last = "last";

        /// <summary>
        /// Indicates that the link's context is a part of a series,
        /// and that the next in the series is the link target.
        /// </summary>
        public const string Next = "next";

        /// <summary>
        /// Indicates that the link's context is a part of a series,
        /// and that the previous in the series is the link target.
        /// </summary>
        public const string Prev = "prev";

        /// <summary>Conveys an identifier for the link's context.</summary>
        public const string Self = "self";

        /// <summary>Refers to a parent document in a hierarchy of documents.</summary>
        public const string Up = "up";
    }
}
