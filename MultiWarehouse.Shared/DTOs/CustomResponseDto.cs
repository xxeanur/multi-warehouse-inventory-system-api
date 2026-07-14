using System.Collections.Generic;

namespace MultiWarehouse.Shared.DTOs
{//Bu dosya, API'nin Next.js frontend'e göndereceği cevapların (JSON) her zaman aynı standart formatta olmasını sağlar. İçinde iki farklı sınıf var.

    // 1. DATA DÖNMEYEN DURUMLAR İÇİN (Örn: Sadece başarılı mesajı veya hata dönerken)

    public class CustomResponseDto
    {
        public bool Success { get; set; }
        public List<string>? ErrorMessage { get; set; }

        public static CustomResponseDto SuccessResponse()
        {
            return new CustomResponseDto { Success = true, ErrorMessage = null };
        }

        public static CustomResponseDto FailResponse(List<string> errorMessage)
        {
            return new CustomResponseDto { Success = false, ErrorMessage = errorMessage };
        }

        public static CustomResponseDto FailResponse(string errorMessage)
        {
            return new CustomResponseDto { Success = false, ErrorMessage = new List<string> { errorMessage } };
        }
    }

    // 2. DATA DÖNEN DURUMLAR İÇİN (Örn: Ürün listesi, Kullanıcı bilgisi dönerken)
    public class CustomResponseDto<T>
    {
        public T? Data { get; set; }
        public bool Success { get; set; }
        public List<string>? ErrorMessage { get; set; }

        public static CustomResponseDto<T> SuccessResponse()//işlem başarılı ama veri yok mesela!!
        {
            return new CustomResponseDto<T> { Success = true, ErrorMessage = null };
        }

        public static CustomResponseDto<T> SuccessResponse(T model)
        {
            return new CustomResponseDto<T> { Success = true, Data = model };
        }

        public static CustomResponseDto<T> FailResponse(List<string> errorMessage)
        {
            return new CustomResponseDto<T> { Success = false, ErrorMessage = errorMessage };
        }

        public static CustomResponseDto<T> FailResponse(string errorMessage)
        {
            return new CustomResponseDto<T> { Success = false, ErrorMessage = new List<string> { errorMessage } };
        }
    }
}