using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Boatanator
{
	public class Sphere
	{
        public VertexBuffer vertexBuffer;
		public IndexBuffer indexBuffer;
        public BasicEffect effect;

		Sphere( ) { }

		public static Sphere Create( GraphicsDevice device )
		{
			return Create( device, 15, 7, v => v );
		}

		public static Sphere CreateNormalTextured( GraphicsDevice device, int slices, int stacks )
		{
			return Create( device, slices, stacks, v => v );
		}

		public static Sphere CreateTextured( GraphicsDevice device, int slices, int stacks )
		{
			return Create( device, slices, stacks, v => new VertexPositionTexture( v.Position, v.TextureCoordinate ) );
		}

		public static Sphere Create<T>( GraphicsDevice device, int slices, int stacks, Func<VertexPositionNormalTexture, T> createVertexCallback ) where T : struct
		{
			return Create( device, slices, stacks, 0, 0, -MathHelper.PiOver2, MathHelper.PiOver2, createVertexCallback, true );
		}

		public static Sphere CreateHalf<T>( GraphicsDevice device, int slices, int stacks, Func<VertexPositionNormalTexture, T> createVertexCallback ) where T : struct
		{
			return Create( device, slices, stacks, 0, 0, 0, MathHelper.PiOver2, createVertexCallback, true );
		}

		public static Sphere Create<T>( GraphicsDevice device, int slices, int stacks,
			float sliceAngle1, float sliceAngle2, float stackAngle1, float stackAngle2,
			Func<VertexPositionNormalTexture, T> createVertexCallback, bool insideVisible ) where T : struct
		{
			if ( slices < 3 || stacks < 3 || slices > ushort.MaxValue || stacks > ushort.MaxValue || ( slices + 1 ) * ( stacks + 1 ) > ushort.MaxValue )
				throw new ArgumentOutOfRangeException( "Sphere does not support 64K+ vertices" );
			if ( sliceAngle1 > sliceAngle2 || stackAngle1 >= stackAngle2 )
				throw new ArgumentOutOfRangeException( "Angles are wrong" );
			var vpnt = new T[ ( slices + 1 ) * ( stacks + 1 ) ];
			float phi, theta;
			float dphi = ( stackAngle2 - stackAngle1 ) / stacks;
			float dtheta = sliceAngle2 == sliceAngle1 ? MathHelper.TwoPi / slices : ( sliceAngle2 - sliceAngle1 ) / slices;
			float x, y, z, sc;
			int index = 0;
			for ( int stack = 0; stack <= stacks; stack++ )
			{
				phi = stack == stacks ? stackAngle2 : stackAngle1 + stack * dphi;
				y = (float)Math.Sin( phi );
				sc = -(float)Math.Cos( phi );
				for ( int slice = 0; slice <= slices; slice++ )
				{
					theta = slice == slices ? sliceAngle2 : sliceAngle1 + slice * dtheta;
					x = sc * (float)Math.Sin( theta );
					z = sc * (float)Math.Cos( theta );
					vpnt[ index++ ] = createVertexCallback( new VertexPositionNormalTexture( new Vector3( x, y, z ), new Vector3( x, y, z ),
						new Vector2( (float)slice / (float)slices, (float)stack / (float)stacks ) ) );
				}
			}
			var indices = new ushort[ slices * stacks * 6 ];
			index = 0;
			int k = slices + 1;
			for ( int stack = 0; stack < stacks; stack++ )
			{
				for ( int slice = 0; slice < slices; slice++ )
				{
					indices[ index++ ] = (ushort)( ( stack + 0 ) * k + slice );
					indices[ index++ ] = insideVisible ? (ushort)( ( stack + 0 ) * k + slice + 1 ) : (ushort)( ( stack + 1 ) * k + slice + 0 );
					indices[ index++ ] = insideVisible ? (ushort)( ( stack + 1 ) * k + slice + 0 ) : (ushort)( ( stack + 0 ) * k + slice + 1 );
					indices[ index++ ] = (ushort)( ( stack + 0 ) * k + slice + 1 );
					indices[ index++ ] = insideVisible ? (ushort)( ( stack + 1 ) * k + slice + 1 ) : (ushort)( ( stack + 1 ) * k + slice );
					indices[ index++ ] = insideVisible ? (ushort)( ( stack + 1 ) * k + slice ) : (ushort)( ( stack + 1 ) * k + slice + 1 );
				}
			}

			var ret = new Sphere( );
			ret.vertexBuffer = new VertexBuffer( device, typeof( T ), vpnt.Length, BufferUsage.WriteOnly );
			ret.vertexBuffer.SetData( vpnt );
			ret.indexBuffer = new IndexBuffer( device, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly );
			ret.indexBuffer.SetData( indices );
			ret.effect = new BasicEffect( device );
			return ret;
		}



		public int Draw( Matrix view, Matrix projection )
		{
			var device = vertexBuffer.GraphicsDevice;
			device.SetVertexBuffer( vertexBuffer );
			device.Indices = indexBuffer;
			effect.View = view;
			effect.Projection = projection;
			effect.CurrentTechnique.Passes[ 0 ].Apply( );
			device.DrawIndexedPrimitives( PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3 );
			return vertexBuffer.VertexCount;
		}
		public int DrawNoShader( )
		{
			var device = vertexBuffer.GraphicsDevice;
			device.SetVertexBuffer( vertexBuffer );
			device.Indices = indexBuffer;
			device.DrawIndexedPrimitives( PrimitiveType.TriangleList, 0, 0, vertexBuffer.VertexCount, 0, indexBuffer.IndexCount / 3 );
			return vertexBuffer.VertexCount;
		}
	}
}
