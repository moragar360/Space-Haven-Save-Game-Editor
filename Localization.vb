Imports System.Globalization
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text.Json
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Controls.Primitives
Imports System.Windows.Data
Imports System.Windows.Media

Namespace SpaceHavenEditor2
    Public Module Localization
        Public Const English As String = "en"
        Public Const SimplifiedChinese As String = "zh-CN"
        Public Const TraditionalChinese As String = "zh-TW"

        Private ReadOnly _tables As New Dictionary(Of String, Dictionary(Of String, String))(StringComparer.Ordinal)
        Private ReadOnly _source As New ConditionalWeakTable(Of DependencyObject, StrongBox(Of String))
        Private ReadOnly _ownedTitles As New HashSet(Of Window)
        Private _language As String = English
        Private _applying As Boolean
        Private _ready As Boolean

        Public Event LanguageChanged()

        Public ReadOnly Property CurrentLanguage As String
            Get
                Return _language
            End Get
        End Property

        Public Sub Initialize()
            If _ready Then Return
            LoadTables()
            Dim saved = My.Settings.Default.Language
            If IsSupported(saved) Then
                _language = saved
            Else
                _language = DetectSystemLanguage()
            End If
            EventManager.RegisterClassHandler(GetType(FrameworkElement),
                                              FrameworkElement.LoadedEvent,
                                              New RoutedEventHandler(AddressOf OnElementLoaded))
            _ready = True
        End Sub

        Public Function T(english As String) As String
            If String.IsNullOrEmpty(english) OrElse _language = English Then Return english
            Dim table As Dictionary(Of String, String) = Nothing
            Dim translated As String = Nothing
            If _tables.TryGetValue(_language, table) AndAlso
               table.TryGetValue(english, translated) AndAlso
               Not String.IsNullOrEmpty(translated) Then
                Return translated
            End If
            Return english
        End Function

        Public Function F(template As String, ParamArray args() As Object) As String
            If args Is Nothing OrElse args.Length = 0 Then Return T(template)
            Return String.Format(CultureInfo.CurrentCulture, T(template), args)
        End Function

        Public Sub SetLanguage(code As String)
            If Not IsSupported(code) Then code = English
            If _language = code Then
                ApplyOpenWindows()
                Return
            End If
            _language = code
            My.Settings.Default.Language = code
            My.Settings.Default.Save()
            ApplyOpenWindows()
            RaiseEvent LanguageChanged()
        End Sub

        Public Sub KeepWindowTitle(window As Window)
            If window Is Nothing Then Return
            _ownedTitles.Add(window)
            _source.Remove(window)
        End Sub

        Public Sub ApplyOpenWindows()
            If Application.Current Is Nothing Then Return
            For Each window As Window In Application.Current.Windows
                ApplyTree(window)
            Next
        End Sub

        Private Sub OnElementLoaded(sender As Object, e As RoutedEventArgs)
            Dim element = TryCast(sender, DependencyObject)
            If element Is Nothing Then Return
            ApplyElement(element)
        End Sub

        Private Sub ApplyTree(root As DependencyObject)
            If root Is Nothing Then Return
            ApplyElement(root)
            For Each child In LogicalTreeHelper.GetChildren(root)
                Dim dep = TryCast(child, DependencyObject)
                If dep IsNot Nothing Then ApplyTree(dep)
            Next
        End Sub

        Private Sub ApplyElement(element As DependencyObject)
            Dim window = TryCast(element, Window)
            If window IsNot Nothing AndAlso Not _ownedTitles.Contains(window) Then
                ApplyStringProperty(window, Window.TitleProperty, window.Title)
            End If

            Dim textBlock = TryCast(element, TextBlock)
            If textBlock IsNot Nothing AndAlso BindingOperations.GetBinding(textBlock, TextBlock.TextProperty) Is Nothing Then
                ApplyStringProperty(textBlock, TextBlock.TextProperty, textBlock.Text)
            End If

            Dim headered = TryCast(element, HeaderedItemsControl)
            If headered IsNot Nothing AndAlso TypeOf headered.Header Is String AndAlso
               BindingOperations.GetBinding(headered, HeaderedItemsControl.HeaderProperty) Is Nothing Then
                ApplyStringProperty(headered, HeaderedItemsControl.HeaderProperty, CStr(headered.Header))
            End If

            Dim headeredContent = TryCast(element, HeaderedContentControl)
            If headeredContent IsNot Nothing AndAlso TypeOf headeredContent.Header Is String AndAlso
               BindingOperations.GetBinding(headeredContent, HeaderedContentControl.HeaderProperty) Is Nothing Then
                ApplyStringProperty(headeredContent, HeaderedContentControl.HeaderProperty, CStr(headeredContent.Header))
            End If

            Dim content = TryCast(element, ContentControl)
            If content IsNot Nothing AndAlso Not TypeOf content Is Window AndAlso TypeOf content.Content Is String AndAlso
               BindingOperations.GetBinding(content, ContentControl.ContentProperty) Is Nothing Then
                ApplyStringProperty(content, ContentControl.ContentProperty, CStr(content.Content))
            End If

            Dim grid = TryCast(element, DataGrid)
            If grid IsNot Nothing Then
                For Each column As DataGridColumn In grid.Columns
                    If TypeOf column.Header Is String Then
                        ApplyStringProperty(column, DataGridColumn.HeaderProperty, CStr(column.Header))
                    End If
                Next
            End If

            Dim framework = TryCast(element, FrameworkElement)
            If framework IsNot Nothing AndAlso TypeOf framework.ToolTip Is String Then
                ApplyStringProperty(framework, FrameworkElement.ToolTipProperty, CStr(framework.ToolTip))
            End If
        End Sub

        Private Sub ApplyStringProperty(target As DependencyObject, [property] As DependencyProperty, current As String)
            If current Is Nothing Then Return
            Dim box As StrongBox(Of String) = Nothing
            Dim source As String
            If _source.TryGetValue(target, box) AndAlso box.Value IsNot Nothing Then
                source = box.Value
            Else
                source = current
                _source.Remove(target)
                _source.Add(target, New StrongBox(Of String)(source))
            End If
            Dim translated = T(source)
            If String.Equals(current, translated, StringComparison.Ordinal) Then Return
            _applying = True
            Try
                target.SetValue([property], translated)
            Finally
                _applying = False
            End Try
        End Sub

        Private Sub LoadTables()
            For Each code In {SimplifiedChinese, TraditionalChinese}
                Dim path = ResolveCatalog(code)
                If path Is Nothing Then Continue For
                Dim map = JsonSerializer.Deserialize(Of Dictionary(Of String, String))(File.ReadAllText(path))
                If map IsNot Nothing Then _tables(code) = map
            Next
        End Sub

        Private Function ResolveCatalog(code As String) As String
            Dim fileName = code & ".json"
            Dim candidates As New List(Of String) From {
                Path.Combine(AppContext.BaseDirectory, "i18n", fileName),
                Path.Combine(Directory.GetCurrentDirectory(), "i18n", fileName)
            }
            Dim dir = New DirectoryInfo(AppContext.BaseDirectory)
            For i = 0 To 4
                If dir Is Nothing Then Exit For
                candidates.Add(Path.Combine(dir.FullName, "i18n", fileName))
                dir = dir.Parent
            Next
            For Each candidate In candidates
                If File.Exists(candidate) Then Return candidate
            Next
            Return Nothing
        End Function

        Private Function DetectSystemLanguage() As String
            Dim name = CultureInfo.CurrentUICulture.Name
            If name.StartsWith("zh-TW", StringComparison.OrdinalIgnoreCase) OrElse
               name.StartsWith("zh-HK", StringComparison.OrdinalIgnoreCase) OrElse
               name.StartsWith("zh-MO", StringComparison.OrdinalIgnoreCase) OrElse
               name.StartsWith("zh-Hant", StringComparison.OrdinalIgnoreCase) Then
                Return TraditionalChinese
            End If
            If name.StartsWith("zh", StringComparison.OrdinalIgnoreCase) Then Return SimplifiedChinese
            Return English
        End Function

        Private Function IsSupported(code As String) As Boolean
            Return code = English OrElse code = SimplifiedChinese OrElse code = TraditionalChinese
        End Function
    End Module
End Namespace
