using System;
using System.Collections.Generic;
using System.Linq;
using SwipeMyRoof.AvaloniaUI.Services.ModelSelection;
using Xunit;

namespace SwipeMyRoof.Tests.UnitTests
{
    public class ModelSelectionTests
    {
        [Fact]
        public void ModelInfo_Constructor_InitializesProperties()
        {
            // Arrange & Act
            var modelInfo = new ModelInfo("openai", "gpt-4");
            
            // Assert
            Assert.Equal("openai", modelInfo.Provider);
            Assert.Equal("gpt-4", modelInfo.Model);
            Assert.Equal("/", modelInfo.Separator);
            Assert.Equal("openai/gpt-4", modelInfo.FullIdentifier);
            Assert.Equal("openai gpt-4", modelInfo.DisplayName);
        }
        
        [Fact]
        public void ModelInfoExtended_GetCost_CalculatesCorrectly()
        {
            // Arrange
            var baseInfo = new ModelInfo("openai", "gpt-4");
            var modelInfo = new ModelInfoExtended(baseInfo, 0.75, 10000, 0.002);
            
            // Act
            var cost = modelInfo.GetCost(5000);
            
            // Assert
            Assert.Equal(10.0, cost); // 5000 * 0.002 = 10.0
        }
        
        [Fact]
        public void ModelSelector_SelectModel_ReturnsHighestQualityModel()
        {
            // Arrange
            var models = new List<ModelInfoExtended>
            {
                new ModelInfoExtended("openai/gpt-3.5-turbo", 0.6, 4000, 0.001),
                new ModelInfoExtended("openai/gpt-4", 0.75, 8000, 0.002),
                new ModelInfoExtended("anthropic/claude-3-opus", 0.9, 16000, 0.003)
            };
            
            var selector = new ModelSelector(models);
            
            // Act - Test with token constraint that allows all models
            var selectedModel = selector.SelectModel(1000, 100.0);
            
            // Assert - Should select the highest quality model
            Assert.Equal("anthropic claude-3-opus", selectedModel.Name);
        }
        
        [Fact]
        public void ModelSelector_SelectModel_RespectsTokenLimit()
        {
            // Arrange
            var models = new List<ModelInfoExtended>
            {
                new ModelInfoExtended("openai/gpt-3.5-turbo", 0.6, 4000, 0.001),
                new ModelInfoExtended("openai/gpt-4", 0.75, 8000, 0.002),
                new ModelInfoExtended("anthropic/claude-3-opus", 0.9, 3000, 0.003)
            };
            
            var selector = new ModelSelector(models);
            
            // Act - Test with token constraint that excludes the highest quality model
            var selectedModel = selector.SelectModel(3500, 100.0);
            
            // Assert - Should select the second highest quality model that meets the token limit
            Assert.Equal("openai gpt-4", selectedModel.Name);
        }
        
        [Fact]
        public void ModelSelector_SelectModel_RespectsBudget()
        {
            // Arrange
            var models = new List<ModelInfoExtended>
            {
                new ModelInfoExtended("openai/gpt-3.5-turbo", 0.6, 4000, 0.001),
                new ModelInfoExtended("openai/gpt-4", 0.75, 8000, 0.002),
                new ModelInfoExtended("anthropic/claude-3-opus", 0.9, 16000, 0.003)
            };
            
            var selector = new ModelSelector(models);
            
            // Act - Test with budget constraint that excludes the highest quality models
            var selectedModel = selector.SelectModel(3000, 3.5);
            
            // Assert - Should select the model that meets the budget constraint
            Assert.Equal("openai gpt-3.5-turbo", selectedModel.Name);
        }
        
        [Fact]
        public void ModelSelector_SelectModel_ThrowsExceptionWhenNoModelMeetsConstraints()
        {
            // Arrange
            var models = new List<ModelInfoExtended>
            {
                new ModelInfoExtended("openai/gpt-3.5-turbo", 0.6, 4000, 0.001),
                new ModelInfoExtended("openai/gpt-4", 0.75, 8000, 0.002)
            };
            
            var selector = new ModelSelector(models);
            
            // Act & Assert - Test with token constraint that excludes all models
            var exception1 = Assert.Throws<InvalidOperationException>(() => 
                selector.SelectModel(10000, 1.0));
            Assert.Contains("No model available", exception1.Message);
            
            // Act & Assert - Test with budget constraint that excludes all models
            var exception2 = Assert.Throws<InvalidOperationException>(() => 
                selector.SelectModel(5000, 0.0001));
            Assert.Contains("No model available", exception2.Message);
        }
    }
}