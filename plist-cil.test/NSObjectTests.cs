using System;
using Claunia.PropertyList;
using Xunit;

namespace plistcil.test
{
    public class NSObjectTests
    {
        #region Scalar types

        [Fact]
        public void DeepCloneNSString()
        {
            NSString source = new NSString("foo");

            NSString clone = (NSString)source.DeepClone();

            // The clone is a distinct instance that holds the same value.
            Assert.NotSame(source, clone);
            Assert.True(source.Equals(clone));
            Assert.Equal("foo", clone.Content);

            // Mutating the clone must not affect the source.
            clone.Content = "bar";
            Assert.Equal("bar", clone.Content);
            Assert.Equal("foo", source.Content);
            Assert.False(source.Equals(clone));
        }

        [Fact]
        public void DeepCloneNSNumberInteger()
        {
            NSNumber source = new NSNumber(42L);

            NSNumber clone = (NSNumber)source.DeepClone();

            Assert.NotSame(source, clone);
            Assert.Equal(NSNumber.INTEGER, clone.GetNSNumberType());
            Assert.Equal(42L, clone.ToLong());
            Assert.True(source.Equals(clone));
        }

        [Fact]
        public void DeepCloneNSNumberReal()
        {
            NSNumber source = new NSNumber(3.5);

            NSNumber clone = (NSNumber)source.DeepClone();

            Assert.NotSame(source, clone);
            Assert.Equal(NSNumber.REAL, clone.GetNSNumberType());
            Assert.Equal(3.5, clone.ToDouble());
            Assert.True(source.Equals(clone));
        }

        [Fact]
        public void DeepCloneNSNumberBoolean()
        {
            NSNumber source = new NSNumber(true);

            NSNumber clone = (NSNumber)source.DeepClone();

            Assert.NotSame(source, clone);
            Assert.Equal(NSNumber.BOOLEAN, clone.GetNSNumberType());
            Assert.True(clone.ToBool());
            Assert.True(source.Equals(clone));
        }

        [Fact]
        public void DeepCloneNSDate()
        {
            DateTime date = new DateTime(1809, 2, 12, 0, 0, 0, DateTimeKind.Utc);
            NSDate source = new NSDate(date);

            NSDate clone = (NSDate)source.DeepClone();

            Assert.NotSame(source, clone);
            Assert.Equal(date, clone.Date);
            Assert.True(source.Equals(clone));
        }

        [Fact]
        public void DeepCloneNSData()
        {
            byte[] bytes = { 1, 2, 3, 4 };
            NSData source = new NSData(bytes);

            NSData clone = (NSData)source.DeepClone();

            Assert.NotSame(source, clone);
            Assert.NotSame(source.Bytes, clone.Bytes);
            Assert.Equal(new byte[] { 1, 2, 3, 4 }, clone.Bytes);
            Assert.True(source.Equals(clone));

            // Mutating the clone's backing array must not affect the source.
            clone.Bytes[0] = 99;
            Assert.Equal(99, clone.Bytes[0]);
            Assert.Equal(1, source.Bytes[0]);
            Assert.False(source.Equals(clone));
        }

        #endregion

        #region NSArray

        [Fact]
        public void DeepCloneNSArray()
        {
            NSString arrayItem = new NSString("foo");
            NSArray  source    = new NSArray(arrayItem);

            NSArray clone = source.DeepClone();

            // Compare values
            Assert.NotSame(source, clone);
            Assert.True(source.Equals(clone));
            Assert.Equal(source.Count, clone.Count);
            Assert.Equal("foo", ((NSString)clone[0]).Content);

            // Compare reference
            Assert.NotSame(source[0], clone[0]);

            // Adding to the clone does not change the source's structure.
            clone.Add(new NSString("bar"));
            Assert.Equal(2, clone.Count);
            Assert.Single(source);
        }

        [Fact]
        public void DeepCloneNSArrayCloneMutation()
        {
            NSArray source = new NSArray(new NSString("foo"));

            NSArray clone = source.DeepClone();

            ((NSString)clone[0]).Content = "changed";

            Assert.Equal("changed", ((NSString)clone[0]).Content);
            Assert.Equal("foo", ((NSString)source[0]).Content);
            Assert.False(source.Equals(clone));
        }

        #endregion

        #region NSDictionary

        [Fact]
        public void DeepCloneNSDictionary()
        {
            NSDictionary source = new NSDictionary
            {
                { "name", new NSString("foo") },
                { "count", new NSNumber(1L) }
            };

            NSDictionary clone = source.DeepClone();

            Assert.NotSame(source, clone);
            Assert.True(source.Equals(clone));
            Assert.Equal(source.Count, clone.Count);
            Assert.Equal("foo", ((NSString)clone["name"]).Content);
            Assert.NotSame(source["name"], clone["name"]);

            // Adding an entry to the clone does not change the source.
            clone.Add("extra", new NSString("bar"));
            Assert.True(clone.ContainsKey("extra"));
            Assert.False(source.ContainsKey("extra"));
        }

        [Fact]
        public void DeepCloneNSDictionaryCloneMutation()
        {
            NSDictionary source = new NSDictionary
            {
                { "name", new NSString("foo") }
            };

            NSDictionary clone = source.DeepClone();

            ((NSString)clone["name"]).Content = "changed";

            Assert.Equal("changed", ((NSString)clone["name"]).Content);
            Assert.Equal("foo", ((NSString)source["name"]).Content);
            Assert.False(source.Equals(clone));
        }

        #endregion

        #region NSSet

        [Fact]
        public void DeepCloneNSSet()
        {
            NSString member = new NSString("foo");
            NSSet    source = new NSSet(member);

            NSSet clone = source.DeepClone();

            Assert.NotSame(source, clone);
            Assert.True(source.Equals(clone));
            Assert.Equal(source.Count, clone.Count);
            Assert.NotSame(source.AllObjects()[0], clone.AllObjects()[0]);

            // Adding to the clone does not change the source.
            clone.AddObject(new NSString("bar"));
            Assert.Equal(2, clone.Count);
            Assert.Single(source);
        }

        #endregion

        #region Nested trees

        [Fact]
        public void DeepCloneNestedTree()
        {
            NSDictionary innerDict = new NSDictionary
            {
                { "nested", new NSNumber(7L) }
            };

            NSArray items = new NSArray(new NSString("foo"), innerDict);

            NSDictionary source = new NSDictionary
            {
                { "items", items },
                { "flag", new NSNumber(true) }
            };

            NSDictionary clone = source.DeepClone();

            // The whole tree compares equal by value.
            Assert.True(source.Equals(clone));

            // Every node along the way is a distinct instance.
            Assert.NotSame(source, clone);
            Assert.NotSame(source["items"], clone["items"]);

            NSArray clonedItems = (NSArray)clone["items"];
            NSArray sourceItems = (NSArray)source["items"];
            Assert.NotSame(sourceItems[0], clonedItems[0]);
            Assert.NotSame(sourceItems[1], clonedItems[1]);

            // Mutate a value buried deep inside the clone.
            NSDictionary clonedInner = (NSDictionary)clonedItems[1];
            ((NSString)clonedItems[0]).Content = "changed";
            clonedInner["nested"] = new NSNumber(99L);

            // The source tree is untouched.
            NSDictionary sourceInner = (NSDictionary)sourceItems[1];
            Assert.Equal("foo", ((NSString)sourceItems[0]).Content);
            Assert.Equal(7L, ((NSNumber)sourceInner["nested"]).ToLong());
            Assert.False(source.Equals(clone));
        }

        #endregion
    }
}
