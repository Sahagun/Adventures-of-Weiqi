using System.Reflection;

namespace EditorAttributes
{
    /// <summary>
    /// Compatibility wrapper for Unity versions that do not expose PropertyAttribute(bool).
    /// </summary>
    public class PropertyAttribute : UnityEngine.PropertyAttribute
    {
        public PropertyAttribute()
        {
        }

        public PropertyAttribute(bool applyToCollection)
        {
            TrySetApplyToCollection(applyToCollection);
        }

        private void TrySetApplyToCollection(bool applyToCollection)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            FieldInfo applyToCollectionField = typeof(UnityEngine.PropertyAttribute).GetField("m_ApplyToCollection", flags);
            if (applyToCollectionField != null)
            {
                TrySetField(applyToCollectionField, applyToCollection);
                return;
            }

            PropertyInfo applyToCollectionProperty = typeof(UnityEngine.PropertyAttribute).GetProperty("applyToCollection", flags);
            if (applyToCollectionProperty != null && applyToCollectionProperty.CanWrite)
                TrySetProperty(applyToCollectionProperty, applyToCollection);
        }

        private void TrySetField(FieldInfo fieldInfo, bool value)
        {
            try
            {
                fieldInfo.SetValue(this, value);
            }
            catch
            {
            }
        }

        private void TrySetProperty(PropertyInfo propertyInfo, bool value)
        {
            try
            {
                propertyInfo.SetValue(this, value);
            }
            catch
            {
            }
        }
    }
}
