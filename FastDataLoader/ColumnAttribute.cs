using System;

namespace FastDataLoader
{
	[AttributeUsage( AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false )]
	public class ColumnAttribute : Attribute
	{
		/// <summary>
		/// <para>Declare name of the column of DataReader to map to the field or property.</para>
		/// <para>If null empty string or whitespace, field or member is not mapping to any column.</para>
		/// </summary>
		/// <param name="name">Column name or null.</param>
		public ColumnAttribute( string name )
		{
			Name = name;
		}

		/// <summary>
		/// Name of the DataReader column to be mapped to.
		/// </summary>
		public string Name { get; private set; }
	}
}
