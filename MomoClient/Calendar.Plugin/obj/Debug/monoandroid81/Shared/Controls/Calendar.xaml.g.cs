//------------------------------------------------------------------------------
// <auto-generated>
//     이 코드는 도구를 사용하여 생성되었습니다.
//     런타임 버전:4.0.30319.42000
//
//     파일 내용을 변경하면 잘못된 동작이 발생할 수 있으며, 코드를 다시 생성하면
//     이러한 변경 내용이 손실됩니다.
// </auto-generated>
//------------------------------------------------------------------------------

[assembly: global::Xamarin.Forms.Xaml.XamlResourceIdAttribute("Xamarin.Plugin.Calendar.Shared.Controls.Calendar.xaml", "Shared/Controls/Calendar.xaml", typeof(global::Xamarin.Plugin.Calendar.Controls.Calendar))]

namespace Xamarin.Plugin.Calendar.Controls {
    
    
    [global::Xamarin.Forms.Xaml.XamlFilePathAttribute("Shared\\Controls\\Calendar.xaml")]
    public partial class Calendar : global::Xamarin.Forms.ContentView {
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::Xamarin.Forms.ContentView calendar;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::Xamarin.Forms.RowDefinition calendarSectionRow;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::Xamarin.Forms.StackLayout calendarContainer;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::Xamarin.Plugin.Calendar.Controls.MonthDaysView monthDaysView;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private global::Xamarin.Forms.ScrollView eventsScrollView;
        
        [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Xamarin.Forms.Build.Tasks.XamlG", "0.0.0.0")]
        private void InitializeComponent() {
            global::Xamarin.Forms.Xaml.Extensions.LoadFromXaml(this, typeof(Calendar));
            calendar = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Xamarin.Forms.ContentView>(this, "calendar");
            calendarSectionRow = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Xamarin.Forms.RowDefinition>(this, "calendarSectionRow");
            calendarContainer = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Xamarin.Forms.StackLayout>(this, "calendarContainer");
            monthDaysView = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Xamarin.Plugin.Calendar.Controls.MonthDaysView>(this, "monthDaysView");
            eventsScrollView = global::Xamarin.Forms.NameScopeExtensions.FindByName<global::Xamarin.Forms.ScrollView>(this, "eventsScrollView");
        }
    }
}
