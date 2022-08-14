using System;
using System.Globalization;
using System.Windows;
using Microsoft.Win32;


namespace ItemCompare
{
    /// <summary>
    /// Looks for tag definition information, taken from the registry.
    /// </summary>
    /// <remarks>
    /// The Surface shell's object-routing feature reads tag definition
    /// information from the registry. We want to use that same information
    /// to drive our UI at run time.
    /// </remarks>
    internal class ByteTagDefinition
    {
        private const string baseRegistryKeyName = "SOFTWARE\\Microsoft\\Surface\\TagInfo\\v1.0\\ByteTags";

        private readonly Vector physicalCenterOffsetFromTag;
        private readonly double orientationOffsetFromTag;

        /// <summary>
        /// Gets the physical center offset from tag.
        /// </summary>
        public Vector PhysicalCenterOffsetFromTag
        {
            get { return physicalCenterOffsetFromTag; }
        }

        /// <summary>
        /// Gets the orientation offset from tag.
        /// </summary>
        public double OrientationOffsetFromTag
        {
            get { return orientationOffsetFromTag; }
        }

        /// <summary>
        /// Gets a double value from a string value stored at a registry key.
        /// </summary>
        /// <param name="key">The registry key where the string value is stored.</param>
        /// <param name="valueName">The name of the registry entry.</param>
        /// <returns>The value as a double, or 0 if the value is not set.</returns>
        private static double GetDoubleFromKey(RegistryKey key , String valueName)
        {
            String valueString = key.GetValue(valueName) as String;
            if (valueString != null)
            {
                return double.Parse(valueString, CultureInfo.InvariantCulture);
            }
            else
            {
                // We will be graceful and return 0 if the value is not set.
                return 0;
            }
        }

        /// <summary>
        /// Looks for a byte tag definition with the specified tag value.
        /// Returns null if not found.
        /// </summary>
        /// <param name="tagValue"></param>
        /// <returns></returns>
        public static ByteTagDefinition Find(byte tagValue)
        {
            string keyName = string.Format(
                CultureInfo.InvariantCulture,
                "{0}\\{1:g}",
                baseRegistryKeyName,
                tagValue);
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyName))
            {
                if (key == null)
                {
                    // tag is not registered
                    return null;
                }

                Vector physicalCenterOffsetFromTag = new Vector(0, 0);
                double orientationOffsetFromTag = 0.0;

                physicalCenterOffsetFromTag.X = GetDoubleFromKey(key, "PhysicalCenterOffsetFromTagX");
                physicalCenterOffsetFromTag.Y = GetDoubleFromKey(key, "PhysicalCenterOffsetFromTagY");
                orientationOffsetFromTag = GetDoubleFromKey(key, "OrientationOffsetFromTag");

                return new ByteTagDefinition(
                    physicalCenterOffsetFromTag,
                    orientationOffsetFromTag);
            }
        }

        /// <summary>
        /// Private constructor.
        /// </summary>
        /// <param name="physicalCenterOffsetFromTag"></param>
        /// <param name="orientationOffsetFromTag"></param>
        private ByteTagDefinition(
            Vector physicalCenterOffsetFromTag,
            double orientationOffsetFromTag)
        {
            this.physicalCenterOffsetFromTag = physicalCenterOffsetFromTag;
            this.orientationOffsetFromTag = orientationOffsetFromTag;
        }
    }
}
