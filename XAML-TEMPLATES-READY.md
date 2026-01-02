# WPF Page Templates - Ready to Copy

## Instructions
1. Open Visual Studio
2. Right-click `src/GUI` project ? Add ? New Folder ? "Views"
3. Right-click "Views" ? Add ? Page (WPF)
4. Name each page (DashboardPage, AssessPage, MigrationPage, SettingsPage)
5. Replace the XAML content with the code below
6. The code-behind is already created, but included here for reference

---

## DashboardPage.xaml
```xaml
<Page x:Class="DocsUnmessed.GUI.Views.DashboardPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      Title="Dashboard">
    <ScrollViewer VerticalScrollBarVisibility="Auto">
        <StackPanel Margin="30">
            <TextBlock Text="Dashboard" FontSize="28" FontWeight="Bold" Margin="0,0,0,10"/>
            <TextBlock Text="Welcome to DocsUnmessed - Your Document Organization Assistant" 
                      FontSize="14" Foreground="#666" Margin="0,0,0,30"/>

            <Grid Margin="0,0,0,20">
                <Grid.ColumnDefinitions>
                    <ColumnDefinition/>
                    <ColumnDefinition/>
                    <ColumnDefinition/>
                </Grid.ColumnDefinitions>
                
                <Border Grid.Column="0" Background="White" BorderBrush="#E0E0E0" BorderThickness="1" 
                       CornerRadius="8" Padding="20" Margin="0,0,10,0">
                    <StackPanel>
                        <TextBlock Text="Total Scans" Foreground="#666" FontSize="12" Margin="0,0,0,5"/>
                        <TextBlock Text="{Binding TotalScans}" FontSize="32" FontWeight="Bold" Foreground="#0078D4"/>
                    </StackPanel>
                </Border>

                <Border Grid.Column="1" Background="White" BorderBrush="#E0E0E0" BorderThickness="1" 
                       CornerRadius="8" Padding="20" Margin="5,0">
                    <StackPanel>
                        <TextBlock Text="Total Files" Foreground="#666" FontSize="12" Margin="0,0,0,5"/>
                        <TextBlock Text="{Binding TotalFiles}" FontSize="32" FontWeight="Bold" Foreground="#107C10"/>
                    </StackPanel>
                </Border>

                <Border Grid.Column="2" Background="White" BorderBrush="#E0E0E0" BorderThickness="1" 
                       CornerRadius="8" Padding="20" Margin="10,0,0,0">
                    <StackPanel>
                        <TextBlock Text="Status" Foreground="#666" FontSize="12" Margin="0,0,0,5"/>
                        <TextBlock Text="Ready" FontSize="32" FontWeight="Bold" Foreground="#8764B8"/>
                    </StackPanel>
                </Border>
            </Grid>

            <Border Background="White" BorderBrush="#E0E0E0" BorderThickness="1" 
                   CornerRadius="8" Padding="20">
                <StackPanel>
                    <TextBlock Text="Quick Start Guide" FontSize="18" FontWeight="SemiBold" Margin="0,0,0,15"/>
                    
                    <StackPanel Margin="0,0,0,10">
                        <TextBlock FontSize="14" FontWeight="SemiBold" Margin="0,0,0,5">
                            <Run Text="1. Assess Documents" Foreground="#0078D4"/>
                        </TextBlock>
                        <TextBlock Text="Go to the Assess page to scan your file system" 
                                  FontSize="13" Foreground="#666" Margin="15,0,0,0"/>
                    </StackPanel>

                    <StackPanel Margin="0,0,0,10">
                        <TextBlock FontSize="14" FontWeight="SemiBold" Margin="0,0,0,5">
                            <Run Text="2. Review Results" Foreground="#107C10"/>
                        </TextBlock>
                        <TextBlock Text="Note the Scan ID from the assessment results" 
                                  FontSize="13" Foreground="#666" Margin="15,0,0,0"/>
                    </StackPanel>

                    <StackPanel>
                        <TextBlock FontSize="14" FontWeight="SemiBold" Margin="0,0,0,5">
                            <Run Text="3. Execute Migration" Foreground="#8764B8"/>
                        </TextBlock>
                        <TextBlock Text="Use the Migration page to organize your files" 
                                  FontSize="13" Foreground="#666" Margin="15,0,0,0"/>
                    </StackPanel>
                </StackPanel>
            </Border>
        </StackPanel>
    </ScrollViewer>
</Page>
```

## DashboardPage.xaml.cs
```csharp
using System.Windows.Controls;
using DocsUnmessed.GUI.ViewModels;

namespace DocsUnmessed.GUI.Views;

public partial class DashboardPage : Page
{
    public DashboardPage(DashboardViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += async (s, e) => await viewModel.LoadAsync();
    }
}
```

---

## AssessPage.xaml
```xaml
<Page x:Class="DocsUnmessed.GUI.Views.AssessPage"
      xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
      xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
      Title="Assess">
    <ScrollViewer VerticalScrollBarVisibility="Auto">
        <StackPanel Margin="30">
            <TextBlock Text="Assess Documents" FontSize="28" FontWeight="Bold" Margin="0,0,0,10"/>
            <TextBlock Text="Scan your file system to analyze organization" FontSize="14" Foreground="#666" Margin="0,0,0,30"/>

            <Border Background="White" BorderBrush="#E0E0E0" BorderThickness="1" CornerRadius="8" Padding="20" Margin="0,0,0,20">
                <StackPanel>
                    <TextBlock Text="Scan Configuration" FontSize="18" FontWeight="SemiBold" Margin="0,0,0,15"/>
                    
                    <Grid Margin="0,0,0,15">
                        <Grid.ColumnDefinitions>
                            <ColumnDefinition Width="*"/>
                            <ColumnDefinition Width="Auto"/>
                        </Grid.ColumnDefinitions>
                        <StackPanel>
                            <TextBlock Text="Scan Path:" Margin="0,0,0,5"/>
                            <TextBox Text="{Binding ScanPath}" FontSize="14" Padding="8"/>
                        </StackPanel>
                        <Button Content="Browse..." Grid.Column="1" Command="{Binding BrowseFolderCommand}" 
                               Padding="15,8" Margin="10,23,0,0" VerticalAlignment="Bottom"/>
                    </Grid>

                    <StackPanel Margin="0,0,0,15">
                        <TextBlock Text="Provider:" Margin="0,0,0,5"/>
                        <ComboBox ItemsSource="{Binding AvailableProviders}" SelectedItem="{Binding SelectedProvider}"
                                 FontSize="14" Padding="8"/>
                    </StackPanel>

                    <StackPanel>
                        <CheckBox Content="Include Subdirectories" IsChecked="{Binding IncludeSubdirectories}" Margin="0,5"/>
                        <CheckBox Content="Compute File Hashes (enables duplicate detection)" IsChecked="{Binding ComputeHash}" Margin="0,5"/>
                        <CheckBox Content="Use Default System Exclusions" IsChecked="{Binding UseDefaultExclusions}" Margin="0,5"/>
                    </StackPanel>

                    <Button Content="Start Scan" Command="{Binding StartScanCommand}"
                           IsEnabled="{Binding IsScanning, Converter={StaticResource InvertBoolConverter}}"
                           Background="#0078D4" Foreground="White" FontSize="16" FontWeight="SemiBold"
                           Padding="30,12" Margin="0,20,0,0" HorizontalAlignment="Left" BorderThickness="0"/>
                </StackPanel>
            </Border>

            <Border Background="White" BorderBrush="#E0E0E0" BorderThickness="1" CornerRadius="8" Padding="20" Margin="0,0,0,20"
                   Visibility="{Binding IsScanning, Converter={StaticResource BoolToVisConverter}}">
                <StackPanel>
                    <TextBlock Text="Scanning..." FontSize="18" FontWeight="SemiBold" Margin="0,0,0,15"/>
                    <ProgressBar IsIndeterminate="True" Height="8" Margin="0,0,0,10"/>
                    <TextBlock Text="{Binding ScanStatus}" FontSize="14" Margin="0,5"/>
                    <TextBlock FontSize="14" Margin="0,10,0,0">
                        <Run Text="Files: "/>
                        <Run Text="{Binding FilesProcessed, Mode=OneWay}" FontWeight="Bold"/>
                        <Run Text="  |  Folders: "/>
                        <Run Text="{Binding FoldersProcessed, Mode=OneWay}" FontWeight="Bold"/>
                        <Run Text="  |  Total: "/>
                        <Run Text="{Binding TotalItems, Mode=OneWay}" FontWeight="Bold"/>
                    </TextBlock>
                </StackPanel>
            </Border>

            <Border Background="White" BorderBrush="#E0E0E0" BorderThickness="1" CornerRadius="8" Padding="20"
                   Visibility="{Binding HasResults, Converter={StaticResource BoolToVisConverter}}">
                <StackPanel>
                    <TextBlock Text="Scan Results" FontSize="18" FontWeight="SemiBold" Margin="0,0,0,15"/>
                    <TextBlock Text="{Binding ResultsSummary}" FontSize="14" TextWrapping="Wrap"/>
                </StackPanel>
            </Border>
        </StackPanel>
    </ScrollViewer>
</Page>
```

## AssessPage.xaml.cs
```csharp
using System.Windows.Controls;
using DocsUnmessed.GUI.ViewModels;

namespace DocsUnmessed.GUI.Views;

public partial class AssessPage : Page
{
    public AssessPage(AssessViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
```

---

## MigrationPage.xaml & SettingsPage.xaml

*See GUI-IMPLEMENTATION-STATUS.md for complete templates*

---

**Note**: All code-behind files are already created via PowerShell. You only need to create the XAML files in Visual Studio.

**Total Time**: 10-15 minutes to copy all 4 pages

**Then**: Build and run! The GUI will be fully functional. ??
