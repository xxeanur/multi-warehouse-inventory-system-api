namespace MultiWarehouse.Shared.DTOs
{

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

    public class CustomResponseDto<T>
    {
        public T? Data { get; set; }
        public bool Success { get; set; }
        public List<string>? ErrorMessage { get; set; }

        public static CustomResponseDto<T> SuccessResponse()
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