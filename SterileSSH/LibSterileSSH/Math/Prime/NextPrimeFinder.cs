using System;
//
// Mono.Math.Prime.Generator.NextPrimeFinder.cs - Prime Generator
//
// Authors:
//	Ben Maurer
//
// Copyright (c) 2003 Ben Maurer. All rights reserved
//
namespace LibSterileSSH.Math.Prime.Generator
{

	/// <summary>
	/// Finds the next prime after a given number.
	/// </summary>
	internal class NextPrimeFinder : SequentialSearchPrimeGeneratorBase
	{

		protected override BigInteger GenerateSearchBase(int bits, object Context)
		{
			if (Context == null)
				throw new ArgumentNullException("Context");
			BigInteger ret = new BigInteger((BigInteger)Context);
			ret.setBit(0);
			return ret;
		}
	}
}
