using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnityEngineX.Tests
{
    [TestFixture]
    public class CircularBufferTests
    {
        [Test]
        public void CircularBuffer_GetEnumeratorConstructorCapacity_ReturnsEmptyCollection()
        {
            var buffer = new CircularBuffer<string>(5);
            CollectionAssert.IsEmpty(buffer.ToArray());
        }

        [Test]
        public void CircularBuffer_ConstructorSizeIndexAccess_CorrectContent()
        {
            var buffer = new CircularBuffer<int>(5, new[] { 0, 1, 2, 3 });

            Assert.That(buffer.Capacity, Is.EqualTo(5));
            Assert.That(buffer.Count, Is.EqualTo(4));
            for (int i = 0; i < 4; i++)
            {
                Assert.That(buffer[i], Is.EqualTo(i));
            }
        }

        [Test]
        public void CircularBuffer_Constructor_ExceptionWhenSourceIsLargerThanCapacity()
        {
            Assert.That(() => new CircularBuffer<int>(3, new[] { 0, 1, 2, 3 }),
                        Throws.Exception.TypeOf<ArgumentException>());
        }

        [Test]
        public void CircularBuffer_GetEnumeratorConstructorDefinedArray_CorrectContent()
        {
            var buffer = new CircularBuffer<int>(5, new[] { 0, 1, 2, 3 });

            int x = 0;
            foreach (var item in buffer)
            {
                Assert.That(item, Is.EqualTo(x));
                x++;
            }
        }

        [Test]
        public void CircularBuffer_PushBack_CorrectContent()
        {
            var buffer = new CircularBuffer<int>(5);

            for (int i = 0; i < 5; i++)
            {
                buffer.PushBack(i);
            }

            Assert.That(buffer.Front(), Is.EqualTo(0));
            for (int i = 0; i < 5; i++)
            {
                Assert.That(buffer[i], Is.EqualTo(i));
            }
        }

        [Test]
        public void CircularBuffer_PushBackOverflowingBuffer_CorrectContent()
        {
            var buffer = new CircularBuffer<int>(5);

            for (int i = 0; i < 10; i++)
            {
                buffer.PushBack(i);
            }

            Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 5, 6, 7, 8, 9 }));
        }

        [Test]
        public void CircularBuffer_GetEnumeratorOverflowedArray_CorrectContent()
        {
            var buffer = new CircularBuffer<int>(5);

            for (int i = 0; i < 10; i++)
            {
                buffer.PushBack(i);
            }

            // buffer should have [5,6,7,8,9]
            int x = 5;
            foreach (var item in buffer)
            {
                Assert.That(item, Is.EqualTo(x));
                x++;
            }
        }

        [Test]
        public void CircularBuffer_ToArrayConstructorDefinedArray_CorrectContent()
        {
            var buffer = new CircularBuffer<int>(5, new[] { 0, 1, 2, 3 });

            Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 0, 1, 2, 3 }));
        }

        [Test]
        public void CircularBuffer_ToArrayOverflowedBuffer_CorrectContent()
        {
            var buffer = new CircularBuffer<int>(5);

            for (int i = 0; i < 10; i++)
            {
                buffer.PushBack(i);
            }

            Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 5, 6, 7, 8, 9 }));
        }

        [Test]
        public void CircularBuffer_ToArraySegmentsConstructorDefinedArray_CorrectContent()
        {
            var buffer = new CircularBuffer<int>(5, new[] { 0, 1, 2, 3 });

            var arraySegments = buffer.ToArraySegments();

            Assert.That(arraySegments.Count, Is.EqualTo(2)); // length of 2 is part of the contract.
            Assert.That(arraySegments.SelectMany(x => x), Is.EqualTo(new[] { 0, 1, 2, 3 }));
        }

        [Test]
        public void CircularBuffer_ToArraySegmentsOverflowedBuffer_CorrectContent()
        {
            var buffer = new CircularBuffer<int>(5);

            for (int i = 0; i < 10; i++)
            {
                buffer.PushBack(i);
            }

            var arraySegments = buffer.ToArraySegments();
            Assert.That(arraySegments.Count, Is.EqualTo(2)); // length of 2 is part of the contract.
            Assert.That(arraySegments.SelectMany(x => x), Is.EqualTo(new[] { 5, 6, 7, 8, 9 }));
        }

        [Test]
        public void CircularBuffer_PushFront_CorrectContent()
        {
            var buffer = new CircularBuffer<int>(5);

            for (int i = 0; i < 5; i++)
            {
                buffer.PushFront(i);
            }

            Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 4, 3, 2, 1, 0 }));
        }

        [Test]
        public void CircularBuffer_PushFrontAndOverflow_CorrectContent()
        {
            var buffer = new CircularBuffer<int>(5);

            for (int i = 0; i < 10; i++)
            {
                buffer.PushFront(i);
            }

            Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 9, 8, 7, 6, 5 }));
        }

        [Test]
        public void CircularBuffer_Front_CorrectItem()
        {
            var buffer = new CircularBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });

            Assert.That(buffer.Front(), Is.EqualTo(0));
        }

        [Test]
        public void CircularBuffer_Back_CorrectItem()
        {
            var buffer = new CircularBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });
            Assert.That(buffer.Back(), Is.EqualTo(4));
        }

        [Test]
        public void CircularBuffer_BackOfBufferOverflowByOne_CorrectItem()
        {
            var buffer = new CircularBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });
            buffer.PushBack(42);
            Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 1, 2, 3, 4, 42 }));
            Assert.That(buffer.Back(), Is.EqualTo(42));
        }

        [Test]
        public void CircularBuffer_Front_EmptyBufferThrowsException()
        {
            var buffer = new CircularBuffer<int>(5);

            Assert.That(() => buffer.Front(),
                         Throws.Exception.TypeOf<InvalidOperationException>().
                         With.Property("Message").Contains("empty buffer"));
        }

        [Test]
        public void CircularBuffer_Back_EmptyBufferThrowsException()
        {
            var buffer = new CircularBuffer<int>(5);
            Assert.That(() => buffer.Back(),
                         Throws.Exception.TypeOf<InvalidOperationException>().
                         With.Property("Message").Contains("empty buffer"));
        }

        [Test]
        public void CircularBuffer_PopBack_RemovesBackElement()
        {
            var buffer = new CircularBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });

            Assert.That(buffer.Count, Is.EqualTo(5));

            buffer.PopBack();

            Assert.That(buffer.Count, Is.EqualTo(4));
            Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 0, 1, 2, 3 }));
        }

        [Test]
        public void CircularBuffer_PopBackInOverflowBuffer_RemovesBackElement()
        {
            var buffer = new CircularBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });
            buffer.PushBack(5);

            Assert.That(buffer.Count, Is.EqualTo(5));
            Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 1, 2, 3, 4, 5 }));

            buffer.PopBack();

            Assert.That(buffer.Count, Is.EqualTo(4));
            Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 1, 2, 3, 4 }));
        }

        [Test]
        public void CircularBuffer_PopFront_RemovesBackElement()
        {
            var buffer = new CircularBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });

            Assert.That(buffer.Count, Is.EqualTo(5));

            buffer.PopFront();

            Assert.That(buffer.Count, Is.EqualTo(4));
            Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 1, 2, 3, 4 }));
        }

        [Test]
        public void CircularBuffer_PopFrontInOverflowBuffer_RemovesBackElement()
        {
            var buffer = new CircularBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });
            buffer.PushFront(5);

            Assert.That(buffer.Count, Is.EqualTo(5));
            Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 5, 0, 1, 2, 3 }));

            buffer.PopFront();

            Assert.That(buffer.Count, Is.EqualTo(4));
            Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 0, 1, 2, 3 }));
        }

        [Test]
        public void CircularBuffer_SetIndex_ReplacesElement()
        {
            var buffer = new CircularBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });

            buffer[1] = 10;
            buffer[3] = 30;

            Assert.That(buffer.ToArray(), Is.EqualTo(new[] { 0, 10, 2, 30, 4 }));
        }

        [Test]
        public void CircularBuffer_WithDifferentSizeAndCapacity_BackReturnsLastArrayPosition()
        {
            // test to confirm this issue does not happen anymore:
            // https://github.com/joaoportela/CircularBuffer-CSharp/issues/2

            var buffer = new CircularBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });

            buffer.PopFront(); // (make size and capacity different)

            Assert.That(buffer.Back(), Is.EqualTo(4));
        }

        [Test]
        public void CircularBuffer_Clear_ClearsContent()
        {
            var buffer = new CircularBuffer<int>(5, new[] { 4, 3, 2, 1, 0 });

            buffer.Clear();

            Assert.That(buffer.Count, Is.EqualTo(0));
            Assert.That(buffer.Capacity, Is.EqualTo(5));
            Assert.That(buffer.ToArray(), Is.EqualTo(new int[0]));
        }

        [Test]
        public void CircularBuffer_Clear_WorksNormallyAfterClear()
        {
            var buffer = new CircularBuffer<int>(5, new[] { 4, 3, 2, 1, 0 });

            buffer.Clear();
            for (int i = 0; i < 5; i++)
            {
                buffer.PushBack(i);
            }

            Assert.That(buffer.Front(), Is.EqualTo(0));
            for (int i = 0; i < 5; i++)
            {
                Assert.That(buffer[i], Is.EqualTo(i));
            }
        }

        [Test]
        public void CircularBuffer_Enumerator_FullNormal()
        {
            var buffer = new CircularBuffer<int>(5, new[] { 0, 1, 2, 3, 4 });

            int index = 0;
            foreach (var val in buffer)
            {
                Assert.That(val, Is.EqualTo(index));
                index++;
            }
            Assert.That(index, Is.EqualTo(5));
        }

        [Test]
        public void CircularBuffer_Enumerator_FullCircular()
        {
            var buffer = new CircularBuffer<int>(5, new[] { 0, 0, 0, 1, 2 });
            for (int i = 3; i < 5; i++)
            {
                buffer.PushBack(i);
            }

            int index = 0;
            foreach (var val in buffer)
            {
                Assert.That(val, Is.EqualTo(index));
                index++;
            }
            Assert.That(index, Is.EqualTo(5));
        }

        [Test]
        public void CircularBuffer_Enumerator_Empty()
        {
            var buffer = new CircularBuffer<int>(5, new int[] { });
            foreach (var val in buffer)
            {
                throw new Exception("Enumerator should not enumerate on empty buffer.");
            }
        }

        [Test]
        public void CircularBuffer_Enumerator_Partial()
        {
            var buffer = new CircularBuffer<int>(5, new int[] { 0, 0, 0, 0, 0 });
            buffer.PushBack(1);
            buffer.PushBack(2);
            buffer.PushBack(3);
            buffer.PopFront();
            int index = 0;
            foreach (var val in buffer)
            {
                Assert.That(val, Is.EqualTo(index));
                index++;
            }
            Assert.That(index, Is.EqualTo(4));
        }

        [Test]
        public void CircularBuffer_BinarySearch_Full_Normal()
        {
            var buffer = new CircularBuffer<float>(5, new float[] { 0, 1, 2, 3, 4 });
            Assert.That(buffer.BinarySearch(0), Is.EqualTo(0));
            Assert.That(buffer.BinarySearch(1), Is.EqualTo(1));
            Assert.That(buffer.BinarySearch(2), Is.EqualTo(2));
            Assert.That(buffer.BinarySearch(3), Is.EqualTo(3));
            Assert.That(buffer.BinarySearch(4), Is.EqualTo(4));

            Assert.That(buffer.BinarySearch(3.5f), Is.EqualTo(~4));
            Assert.That(buffer.BinarySearch(6), Is.EqualTo(~5));
            Assert.That(new List<float>() { 0, 1, 2, 3, 4 }.BinarySearch(-1), Is.EqualTo(~0));
            Assert.That(buffer.BinarySearch(-1), Is.EqualTo(~0));
        }

        [Test]
        public void CircularBuffer_BinarySearch_Full_Circular()
        {
            var buffer = new CircularBuffer<float>(5, new float[] { 0, 1, 2, 3, 4 });
            buffer.PushBack(5);
            buffer.PushBack(6);
            Assert.That(buffer.BinarySearch(2), Is.EqualTo(0));
            Assert.That(buffer.BinarySearch(3), Is.EqualTo(1));
            Assert.That(buffer.BinarySearch(4), Is.EqualTo(2));
            Assert.That(buffer.BinarySearch(5), Is.EqualTo(3));
            Assert.That(buffer.BinarySearch(6), Is.EqualTo(4));

            Assert.That(buffer.BinarySearch(9), Is.EqualTo(~5));
            Assert.That(buffer.BinarySearch(3.5f), Is.EqualTo(~2));
            Assert.That(buffer.BinarySearch(4.5f), Is.EqualTo(~3));
            Assert.That(buffer.BinarySearch(-1), Is.EqualTo(~0));
        }

        [Test]
        public void CircularBuffer_BinarySearch_Partial_Circular()
        {
            var buffer = new CircularBuffer<float>(10, new float[] { 5, 6 });
            buffer.PushFront(4);
            buffer.PushFront(3);
            buffer.PushFront(2);

            Assert.That(buffer.BinarySearch(2), Is.EqualTo(0));
            Assert.That(buffer.BinarySearch(3), Is.EqualTo(1));
            Assert.That(buffer.BinarySearch(4), Is.EqualTo(2));
            Assert.That(buffer.BinarySearch(5), Is.EqualTo(3));
            Assert.That(buffer.BinarySearch(6), Is.EqualTo(4));

            Assert.That(buffer.BinarySearch(9), Is.EqualTo(~5));
            Assert.That(buffer.BinarySearch(3.5f), Is.EqualTo(~2));
            Assert.That(buffer.BinarySearch(4.5f), Is.EqualTo(~3));
            Assert.That(buffer.BinarySearch(-1), Is.EqualTo(~0));
        }

        [Test]
        public void CircularBuffer_BinarySearch_Partial_Normal()
        {
            var buffer = new CircularBuffer<float>(10, new float[] { 0, 0, 1 });
            buffer.PushBack(2);
            buffer.PushBack(3);
            buffer.PushBack(4);
            buffer.PopFront();
            Assert.That(buffer.BinarySearch(0), Is.EqualTo(0));
            Assert.That(buffer.BinarySearch(1), Is.EqualTo(1));
            Assert.That(buffer.BinarySearch(2), Is.EqualTo(2));
            Assert.That(buffer.BinarySearch(3), Is.EqualTo(3));
            Assert.That(buffer.BinarySearch(4), Is.EqualTo(4));

            Assert.That(buffer.BinarySearch(3.5f), Is.EqualTo(~4));
            Assert.That(buffer.BinarySearch(6), Is.EqualTo(~5));
            Assert.That(new List<float>() { 0, 1, 2, 3, 4 }.BinarySearch(-1), Is.EqualTo(~0));
            Assert.That(buffer.BinarySearch(-1), Is.EqualTo(~0));
        }
    }
}
