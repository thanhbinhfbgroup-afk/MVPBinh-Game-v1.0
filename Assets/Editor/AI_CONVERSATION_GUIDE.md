# AI Conversation Guide - BillGameCore Tung

File này dành cho AI hội thoại chỉ tư vấn, review, phân tích hướng làm hoặc trả lời câu hỏi kiến trúc. AI đọc file này không mặc định được phép sửa code.

## 1. Read Order

1. Đọc `Assets/Editor/CONTEXT.v2.md` trước. Đây là luật kiến trúc gốc.
2. Đọc file này sau để biết cách trả lời đúng kiểu hội thoại.
3. Đọc thêm script/folder liên quan do người dùng cung cấp.

Nếu không đọc đủ context hoặc thiếu file liên quan, phải nói rõ phần nào đang là giả định.

## 2. Project Summary

BillGameCore là Unity 2D game dùng VContainer và New Input System.

Kiến trúc hiện tại:
- Feature-first, không layer-first.
- `Core` BCL-only, `noEngineReferences = true`.
- Cross-module contract đi qua `SharedPorts`.
- Scene startup nằm trong `Scenes`, không nằm trong `Composition`.
- Per-entity runtime object do spawner/binder tạo, không register vào DI.
- `InputReader` dùng `InputActionGateway`, không dùng `PlayerInput` component và không dùng Generate C# wrapper.

Baseline module hiện tại:
- `Core`
- `SharedPorts`
- `Modules/Input`
- `Modules/Player`
- `Modules/Inventory`
- `Modules/InteractionGroup/Chest`
- `Modules/Enemy`
- `Composition`
- `Scenes`

## 3. Answering Rules

AI hội thoại phải:
- Trả lời theo rule trong `CONTEXT.v2.md`.
- Nêu rõ kết luận trước, sau đó mới giải thích.
- Gắn đề xuất với module/folder cụ thể.
- Phân biệt rõ Project scope, Scene scope, per-entity runtime và ScriptableObject config.
- Nói rõ khi một đề xuất cần sửa asmdef reference.
- Hỏi lại nếu thiếu thông tin có thể làm sai kiến trúc.

AI hội thoại không được đề xuất:
- Singleton gameplay system.
- `Find()`, `FindObjectOfType()`, scene locator.
- Module A gọi namespace nội bộ Module B.
- `Composition` reference `Scenes`.
- Đưa runtime state/application/presenter/runtime vào DI.
- Đưa interface vào `Core` khi mới chỉ 1-2 module dùng.
- Dùng MessagePipe làm backbone khi direct callback/interface đang đủ.
- Gắn `PlayerInput` component vào scene object `Input`.
- Bật Generate C# wrapper trên `.inputactions`.
- Sinh module generic nếu chưa có slice spec rõ.

## 4. How To Answer Architecture Questions

Khi người dùng hỏi "nên đặt class này ở đâu?", trả lời theo thứ tự:

1. Class này thuộc module nào.
2. Nằm trong layer nào: `Domain`, `Application`, `Infrastructure`, `Presentation`, `SharedPorts`, `Scenes`, hoặc `Composition`.
3. Có cần asmdef reference mới không.
4. Có phá rule nào trong `CONTEXT.v2.md` không.
5. Nếu còn thiếu thông tin, hỏi lại đúng điểm thiếu.

Khi người dùng hỏi "có nên thêm interface vào Core không?", mặc định trả lời:
- Không, trừ khi nó là contract thật sự dùng chung rộng, BCL-only, không phụ thuộc Unity/package/module.
- Nếu chỉ để module nói chuyện với module khác, ưu tiên `SharedPorts`.
- Nếu chỉ dùng nội bộ một module, đặt trong `Application/Ports` của module đó.

Khi người dùng hỏi "flow này có ổn không?", kiểm tra:
- Có đi xuyên module bằng concrete class không.
- Có tạo vòng asmdef không.
- Có nhét scene orchestration vào `Composition` không.
- Có register per-entity runtime vào DI không.
- Có phụ thuộc UnityEngine trong `Core` hoặc Application/Domain không.

## 5. Good Answer Format

Ưu tiên format ngắn:

```text
Kết luận: ...

Lý do:
- ...
- ...

Nên sửa/đặt ở:
- ...

Rủi ro:
- ...

Cần hỏi lại:
- ...
```

Nếu câu hỏi nhỏ, có thể trả lời gọn hơn nhưng vẫn phải đúng rule.

## 6. Prompt Template For Conversation AI

Bạn có thể copy mẫu này khi hỏi AI hội thoại:

```text
Bạn đang tư vấn kiến trúc cho Unity project BillGameCore.
Hãy đọc và tuân thủ:
- Assets/Editor/CONTEXT.v2.md
- Assets/Editor/AI_CONVERSATION_GUIDE.md

Không tự ý đề xuất sửa code nếu chưa được hỏi.
Không dùng Singleton/Find/Manager pattern.
Không đề xuất PlayerInput component hoặc Generate C# wrapper.

Câu hỏi của tôi:
...

File/code liên quan:
...
```

## 7. Conversation AI Output Contract

AI hội thoại nên luôn nói rõ:
- Kết luận.
- Rule/context liên quan.
- File/folder liên quan.
- Rủi ro compile/runtime nếu có.
- Điều cần hỏi lại nếu thiếu dữ liệu.

Nếu AI không chắc, phải nói "chưa đủ thông tin để khẳng định" và yêu cầu file cụ thể.
