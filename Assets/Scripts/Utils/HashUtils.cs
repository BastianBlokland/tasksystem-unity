namespace Utils
{
	public static class HashUtils
	{
		public static int GetBucket(int hash, int bucketCount)
        {
			//'& 0x7fffffff' is a very fast way to make a integer absolute (just removing the sign)
            return (hash & 0x7fffffff) % bucketCount;
        }
	}
}