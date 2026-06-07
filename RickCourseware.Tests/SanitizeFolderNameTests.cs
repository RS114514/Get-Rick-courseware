using Xunit;
using USBAutoCopy;

namespace RickCourseware.Tests
{
    public class SanitizeFolderNameTests
    {
        [Theory]
        [InlineData(null, "未命名")]
        [InlineData("", "未命名")]
        [InlineData("   ", "未命名")]
        [InlineData("正常名称", "正常名称")]
        [InlineData("名称 带有 空格", "名称 带有 空格")]
        [InlineData("带有非法字符\\/:*?\"<>|", "带有非法字符_________")]
        [InlineData("超长文件名123456789012345678901234567890这部分应该被截断", "超长文件名1234567890123456789012345")]
        [InlineData("超长文件名且截断后末尾是空格                  ", "超长文件名且截断后末尾是空格")]
        [InlineData("   首尾带空格名称   ", "首尾带空格名称")]
        [InlineData("a:b\\c/d", "a_b_c_d")]
        public void TestSanitizeFolderName(string? input, string expected)
        {
            var result = USBMonitor.SanitizeFolderName(input);
            Assert.Equal(expected, result);
        }
    }
}
