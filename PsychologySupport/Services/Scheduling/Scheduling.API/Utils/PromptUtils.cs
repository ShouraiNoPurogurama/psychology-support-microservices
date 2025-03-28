namespace Scheduling.API.Utils;

public static class PromptUtils
{
    public static string GetCreateScheduleTemplate(string userPreferences, string clusteringPattern, string clusters)
    {
        var template = """
                                   Bạn là một chuyên gia xử lý dữ liệu có nhiệm vụ xây dựng lịch trình cá nhân hóa cho từng ngày trong 14 ngày. Thực hiện các bước sau và trả về kết quả dưới dạng JSON, không cần giải thích:
                       
                                   1. **Phân tích đầu vào**:
                                      - Đọc dữ liệu `clusteringPattern` để hiểu cách phân cụm hoạt động.
                                      - Đọc danh sách `clusters` để biết các nhóm hoạt động có sẵn.
                                      - Xem xét `userPreferences` để hiểu điểm đối với các triệu chứng Depression, Anxiety và Stress mà người dùng mắc phải, mức độ và điểm các triệu chứng dựa trên thang bài test DASS21.
                       
                                   2. **Chọn cụm phù hợp**:
                                      - Dựa trên `userPreferences`, xác định một cụm hoạt động phù hợp nhất từ `clusters`.
                                      - Cụm được chọn phải đảm bảo đa dạng về hoạt động nhưng vẫn ưu tiên các yếu tố quan trọng với người dùng.
                       
                                   3. **Tạo lịch trình 14 ngày**:
                                      - Mỗi ngày có một phiên (session).
                                      - Mỗi phiên gồm:
                                        - 3 hoạt động ăn uống (`foodActivity`).
                                        - 2 hoạt động thể chất (`physicalActivity`).
                                        - 1 hoạt động trị liệu (`therapeuticActivity`).
                                        - 1 hoạt động giải trí (`entertainmentActivity`).
                                      - Các hoạt động cần có sự liên kết hợp lý và đảm bảo trải nghiệm tích cực.
                       
                                   4. **Xuất kết quả dưới dạng JSON** với cấu trúc:
                                   {
                                     "startDate": "<YYYY-MM-DDTHH:MM:SS>",
                                     "endDate": "<YYYY-MM-DDTHH:MM:SS>",
                                     "sessions": [
                                       {
                                         "id": "<GUID>",
                                         "scheduleId": "<GUID>",
                                         "purpose": "<Purpose>",
                                         "order": <Number>,
                                         "startDate": "<YYYY-MM-DDTHH:MM:SS>",
                                         "endDate": "<YYYY-MM-DDTHH:MM:SS>",
                                         "activities": [
                                           {
                                             "id": "<GUID>",
                                             "sessionId": "<GUID>",
                                             "description": "<Activity Description>",
                                             "timeRange": "<YYYY-MM-DDTHH:MM:SS>",
                                             "duration": "<Duration>",
                                             "dateNumber": <Number>,
                                             "status": "<Planned | Completed | Ongoing>",
                                             "entertainmentActivity": { "id": "<GUID>", "name": "<Activity Name>" },
                                             "foodActivity": { "id": "<GUID>", "name": "<Food Name>" },
                                             "physicalActivity": { "id": "<GUID>", "name": "<Physical Activity>" },
                                             "therapeuticActivity": { "id": "<GUID>", "name": "<Therapy Name>" }
                                           }
                                         ]
                                       }
                                     ],
                                     "status": "Scheduled"
                                   }
                       
                                   **Yêu cầu quan trọng**:
                                   - **Ưu tiên các hoạt động có lợi nhất** theo sở thích của người dùng.
                                   - **Mục đích của từng session** phải phản ánh đúng ý nghĩa của cụm được chọn.
                                   - **Đảm bảo JSON đầu ra đúng định dạng** để sử dụng trực tiếp.
                                   - ** Ngày tháng phải bắt đầu từ 28/03/2025, với format id chung "id": <guid>,
                       """
                       +
                       $"""
                            **Dữ liệu đầu vào**:
                            - `userPreferences`: {userPreferences}
                            - `clusteringPattern`: {clusteringPattern}
                            - `clusters`: {clusters}
                        """;

        var template2 = """
                                    Bạn là một chuyên gia xử lý dữ liệu có nhiệm vụ xây dựng lịch trình cá nhân hóa trong 14 ngày. Thực hiện các bước sau và trả về kết quả dưới dạng JSON, không cần giải thích:
                        
                                    1. **Phân tích đầu vào**:
                                       - Đọc dữ liệu `clusteringPattern` từ những prompt cũ để hiểu cách phân cụm hoạt động.
                                       - Đọc danh sách `clusters` từ những prompt cũ để biết các nhóm hoạt động có sẵn.
                                       - Xem xét `userPreferences` để hiểu điểm đối với các triệu chứng Depression, Anxiety và Stress mà người dùng mắc phải, mức độ và điểm các triệu chứng dựa trên thang bài test DASS21.
                        
                                    2. **Tạo lịch trình 14 ngày**:
                                       - Mỗi ngày có một phiên (session).
                                       - Mỗi phiên gồm:
                                         - 3 hoạt động ăn uống (`foodActivity`).
                                         - 2 hoạt động thể chất (`physicalActivity`).
                                         - 1 hoạt động trị liệu (`therapeuticActivity`).
                                         - 1 hoạt động giải trí (`entertainmentActivity`).
                                       - Các hoạt động cần có sự liên kết hợp lý và đảm bảo trải nghiệm tích cực.
                        
                                    3. **Xuất kết quả dưới dạng JSON** với cấu trúc:
                                    {
                          "startDate": "2025-03-28T00:00:00",
                          "endDate": "2025-04-10T23:59:59",
                          "sessions": [
                            {
                              "id": "<UUID>",
                              "scheduleId": "<UUID>",
                              "purpose": "<Purpose Based on Cluster>",
                              "order": <Number>,
                              "startDate": "2025-03-28T00:00:00",
                              "endDate": "2025-03-28T23:59:59",
                              "activities": [
                                {
                                  "id": "<UUID>",
                                  "sessionId": "<UUID>",
                                  "description": "<Activity Description>",
                                  "timeRange": "<YYYY-MM-DDTHH:MM:SS>",
                                  "duration": "<Duration>",
                                  "dateNumber": <Number>,
                                  "status": "Planned",
                                  "entertainmentActivity": { "id": "<UUID>", "name": "<Activity Name>" },
                                  "foodActivity": { "id": "<UUID>", "name": "<Food Name>" },
                                  "physicalActivity": { "id": "<UUID>", "name": "<Physical Activity>" },
                                  "therapeuticActivity": { "id": "<UUID>", "name": "<Therapy Name>" }
                                }
                              ]
                            }
                          ],
                          "status": "Scheduled"
                        }
                        
                        
                                    **Yêu cầu quan trọng**:
                                    - **Ưu tiên các hoạt động có lợi nhất** theo sở thích của người dùng.
                                    - **Mục đích của từng session** phải phản ánh đúng ý nghĩa của cụm được chọn.
                                    - **Đảm bảo JSON đầu ra đúng định dạng** để sử dụng trực tiếp.
                                    - ** Ngày tháng phải bắt đầu từ 28/03/2025
                        """
                        +
                        $"""
                             **Dữ liệu đầu vào**:
                             - `userPreferences`: {userPreferences}
                             - `clusteringPattern`
                             - `clusters`
                         """;

        return template2;
    }
}