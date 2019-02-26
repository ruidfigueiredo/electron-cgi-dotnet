using System;
using System.Collections.Generic;
using System.Linq;
using Moq;
using Xunit;

namespace ElectronCgi.DotNet.Tests
{
    public class TabSeparatedInputStreamParserTests
    {
        [Fact]
        public void AddPartial_EmptyString_NoFrames()
        {            
            var parser = new TabSeparatedInputStreamParser();
            
            parser.AddPartial(string.Empty);

            Assert.False(parser.HasCompletedFrames);
            Assert.Empty(parser.GetCompletedFrames());            
        }

        [Fact]
        public void AddPartial_PartialRequest_NoFrames()
        {
            var parser = new TabSeparatedInputStreamParser();

            parser.AddPartial("{\"type\":\"the type\",\"id");

            Assert.False(parser.HasCompletedFrames);
            Assert.Empty(parser.GetCompletedFrames());            
        }

        [Fact]
        public void AddPartial_CompleteRequestWithSeparator_HasOneFrame()
        {
            var parser = new TabSeparatedInputStreamParser();

            parser.AddPartial("{\"type\":\"requestType\"}\t");
            var frames = parser.GetCompletedFrames().ToArray();

            Assert.True(parser.HasCompletedFrames);
            Assert.Single(frames);
            Assert.Equal("{\"type\":\"requestType\"}", frames[0]);
        }

        [Fact]
        public void AddPartial_CompleteRequestAndSeparatorFollowedByPartialRequest_OneFrameAvailable()
        {
            var parser = new TabSeparatedInputStreamParser();
            
            parser.AddPartial("{\"type\":\"requestType\"}\t{\"type\":\"Incomplete request...");
            var frames = parser.GetCompletedFrames().ToArray();

            Assert.True(parser.HasCompletedFrames);
            Assert.Single(frames);
            Assert.Equal("{\"type\":\"requestType\"}", frames[0]);
        }


        [Fact]
        public void AddPartial_CompleteRequestAndSeparatorFollowedBySeparatePartialRequest_OneFrameAvailable()
        {
            var parser = new TabSeparatedInputStreamParser();
            
            parser.AddPartial("{\"type\":\"requestType\"}\t");
            parser.AddPartial("{\"type\":\"Incomplete request...");
            var frames = parser.GetCompletedFrames().ToArray();

            Assert.True(parser.HasCompletedFrames);
            Assert.Single(frames);
            Assert.Equal("{\"type\":\"requestType\"}", frames[0]);
        }


        [Fact]
        public void AddPartial_TwoCompleteFramesSeparatedByTabs_HasTwoFrames()
        {
            var parser = new TabSeparatedInputStreamParser();

            parser.AddPartial("request1\t");
            parser.AddPartial("request2\t");
            var result = parser.GetCompletedFrames().ToArray();
            
            Assert.Equal(2, result.Length);
            Assert.True(parser.HasCompletedFrames);
            Assert.Equal("request1", result[0]);
            Assert.Equal("request2", result[1]);            
        }

        //Clear removes all completed frames
        //Clear does has no influence on what GetCompletedFrames returns        
    }
}
