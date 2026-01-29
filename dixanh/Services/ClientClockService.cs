using Microsoft.JSInterop;

namespace dixanh.Services
{
    public interface IClientClockService
    {
        ValueTask<string> GetTimeZoneAsync();
        ValueTask<int> GetOffsetNowMinutesAsync();
        ValueTask<int> GetOffsetAtLocalMinutesAsync(string isoLocal);
    }

    public sealed class ClientClockService : IClientClockService
    {
        private readonly IJSRuntime _js;
        public ClientClockService(IJSRuntime js) => _js = js;

        // Lấy múi giờ của client (ví dụ: "Asia/Ho_Chi_Minh")
        // Cần đảm bảo đã load getClockClient JS trước khi gọi
        public ValueTask<string> GetTimeZoneAsync()
            => _js.InvokeAsync<string>("getClockClient.timeZone");

        // Lấy offset hiện tại của client so với UTC, đơn vị minutes
        // VD: Việt Nam UTC+7 => -420
        // Cần đảm bảo đã load getClockClient JS trước khi gọi
        public ValueTask<int> GetOffsetNowMinutesAsync()
            => _js.InvokeAsync<int>("getClockClient.offsetNow");

        // Lấy offset của client so với UTC tại thời điểm local được cung cấp, đơn vị minutes
        // isoLocal format: "yyyy-MM-ddTHH:mm:ss" (hoặc không có seconds)
        // VD: Việt Nam UTC+7 => -420
        public ValueTask<int> GetOffsetAtLocalMinutesAsync(string isoLocal)
            => _js.InvokeAsync<int>("getClockClient.offsetAtLocal", isoLocal);
    }
}

/*
 
    4.1. Hiển thị giờ theo client
    Ví dụ trong /vehicles bạn muốn hiển thị CreatedAt đúng giờ client:

        @inject dixanh.Services.IClientClock ClientClock
        @using dixanh.Extensions

        @code {
            private int _clientOffsetNow;
            protected override async Task OnAfterRenderAsync(bool firstRender)
            {
                if (!firstRender) return;
                _clientOffsetNow = await ClientClock.GetOffsetNowMinutesAsync();
                StateHasChanged();
            }
        }

    Trong table:
        <td class="px-4 py-3">@v.CreatedAt.ToClientString(_clientOffsetNow)</td>
        <td class="px-4 py-3">@v.UpdatedAt.ToClientString(_clientOffsetNow)</td>
    Gợi ý: DB lưu UTC; lúc render chỉ cần ToClientString(offset).
 
 */