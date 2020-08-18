//
// Copyright (c) 2019-2020 Ryujinx
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
//

namespace Ryujinx.Audio.Renderer.Server.Effect
{
    /// <summary>
    /// The usage state of an effect.
    /// </summary>
    public enum UsageState : byte
    {
        /// <summary>
        /// The effect is in an invalid state.
        /// </summary>
        Invalid,

        /// <summary>
        /// The effect is new.
        /// </summary>
        New,

        /// <summary>
        /// The effect is enabled.
        /// </summary>
        Enabled,

        /// <summary>
        /// The effect is disabled.
        /// </summary>
        Disabled
    }
}
