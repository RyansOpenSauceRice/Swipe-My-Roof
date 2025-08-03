using System;
using System.Threading.Tasks;
using SwipeMyRoof.AvaloniaUI.ViewModels;
using Xunit;

namespace SwipeMyRoof.Tests.UnitTests
{
    public class BuildingValidationViewModelTests
    {
        [Fact]
        public void Constructor_InitializesProperties()
        {
            // Arrange & Act
            var viewModel = new BuildingValidationViewModel();
            
            // Assert
            Assert.NotNull(viewModel.ConfidenceIndicator);
            Assert.True(viewModel.ShowConfidenceIndicator);
            Assert.False(viewModel.IsNarrowView);
            Assert.True(viewModel.IsLoadingImage);
            Assert.Null(viewModel.BuildingImage);
            Assert.NotNull(viewModel.CurrentBuilding);
            Assert.Equal(123456, viewModel.CurrentBuilding.OsmId);
        }
        
        [Fact]
        public void HandleWindowSizeChanged_SetsIsNarrowViewCorrectly()
        {
            // Arrange
            var viewModel = new BuildingValidationViewModel();
            
            // Act - Test with narrow width
            viewModel.HandleWindowSizeChanged(700);
            
            // Assert
            Assert.True(viewModel.IsNarrowView);
            
            // Act - Test with wide width
            viewModel.HandleWindowSizeChanged(1000);
            
            // Assert
            Assert.False(viewModel.IsNarrowView);
        }
        
        [Fact]
        public async Task AcceptBuildingCommand_CallsLoadNextBuilding()
        {
            // Arrange
            var viewModel = new BuildingValidationViewModel();
            var initialBuildingId = viewModel.CurrentBuilding.OsmId;
            
            // Act
            await viewModel.AcceptBuildingCommand.ExecuteAsync(null);
            
            // Assert
            Assert.NotEqual(initialBuildingId, viewModel.CurrentBuilding.OsmId);
        }
        
        [Fact]
        public async Task RejectBuildingCommand_CallsLoadNextBuilding()
        {
            // Arrange
            var viewModel = new BuildingValidationViewModel();
            var initialBuildingId = viewModel.CurrentBuilding.OsmId;
            
            // Act
            await viewModel.RejectBuildingCommand.ExecuteAsync(null);
            
            // Assert
            Assert.NotEqual(initialBuildingId, viewModel.CurrentBuilding.OsmId);
        }
        
        [Fact]
        public async Task SkipBuildingCommand_CallsLoadNextBuilding()
        {
            // Arrange
            var viewModel = new BuildingValidationViewModel();
            var initialBuildingId = viewModel.CurrentBuilding.OsmId;
            
            // Act
            await viewModel.SkipBuildingCommand.ExecuteAsync(null);
            
            // Assert
            Assert.NotEqual(initialBuildingId, viewModel.CurrentBuilding.OsmId);
        }
        
        [Fact]
        public void GetPracticeModeBanner_ReturnsCorrectText()
        {
            // Arrange
            var viewModel = new BuildingValidationViewModel();
            
            // Act
            var bannerText = viewModel.GetPracticeModeBanner;
            
            // Assert
            Assert.Contains("PRACTICE MODE", bannerText);
            Assert.Contains("OpenStreetMap", bannerText);
        }
    }
}